using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Contracts;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromBody] AuthRequestCodeRequest request)
    {
        var email = request.Email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return BadRequest("Invalid email");

        // cooldown: 2 минуты
        var last = await _db.EmailVerificationCodes
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (last != null)
        {
            var seconds = (int)(DateTimeOffset.UtcNow - last.CreatedAt).TotalSeconds;
            if (seconds < 120)
                return BadRequest($"Wait {120 - seconds} seconds");
        }

        var code = CodeGenerator.Generate4Digits();
        var hash = HashService.Sha256($"{email}:{code}");

        var entity = new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            Email = email,
            CodeHash = hash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Attempts = 0,
            IsUsed = false
        };

        _db.EmailVerificationCodes.Add(entity);
        await _db.SaveChangesAsync();

        // Временно: выводим код в консоль (потом заменим на отправку email)
        Console.WriteLine($"[AUTH CODE] {email} => {code}");

        return Ok(new { cooldownSeconds = 120 });
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] AuthVerifyCodeRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return BadRequest("Invalid email");

        if (code.Length != 4 || !code.All(char.IsDigit))
            return BadRequest("Invalid code");

        var entity = await _db.EmailVerificationCodes
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity == null)
            return BadRequest("Code not found");

        if (entity.IsUsed)
            return BadRequest("Code already used");

        if (DateTimeOffset.UtcNow > entity.ExpiresAt)
            return BadRequest("Code expired");

        if (entity.Attempts >= 5)
            return BadRequest("Too many attempts");

        entity.Attempts++;

        var inputHash = HashService.Sha256($"{email}:{code}");

        if (entity.CodeHash != inputHash)
        {
            await _db.SaveChangesAsync();
            return BadRequest("Invalid code");
        }

        entity.IsUsed = true;
        await _db.SaveChangesAsync();

        // 1) найти или создать пользователя
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                CreatedAt = DateTimeOffset.UtcNow,
                Points = 0,
                FirstName = null,
                LastName = null
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        // 2) выпустить токен
        var token = _jwt.CreateToken(user);

        var needsProfile = string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName);

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Points
            },
            needsProfile
        });
    }

    [Authorize]
    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirstValue("uid"); // из токена
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();

        if (firstName.Length < 2 || lastName.Length < 2)
            return BadRequest("FirstName/LastName must be at least 2 characters");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound("User not found");

        user.FirstName = firstName;
        user.LastName = lastName;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        // берём userId из токена
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound("User not found");

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Points
        });
    }

    [Authorize]
    [HttpPatch("profile")]
    public async Task<IActionResult> PatchProfile([FromBody] PatchProfileRequest request)
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound("User not found");

        // обновляем только те поля, что пришли
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            if (request.FirstName.Trim().Length < 2)
                return BadRequest("FirstName must be at least 2 characters");
            user.FirstName = request.FirstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            if (request.LastName.Trim().Length < 2)
                return BadRequest("LastName must be at least 2 characters");
            user.LastName = request.LastName.Trim();
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Points
        });
    }

    [Authorize]
    [HttpPost("request-change-email")]
    public async Task<IActionResult> RequestChangeEmail([FromBody] AuthRequestChangeEmailRequest request)
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var newEmail = request.NewEmail.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
            return BadRequest("Invalid email");

        //нельзя поставить email, который уже занят
        var exists = await _db.Users.AnyAsync(X => X.Email == newEmail);
        if (exists)
            return BadRequest("Email elredy used");

        //cooldown (например 2 минуты на один email)
        var last = await _db.EmailChangeCodes
             .Where(x => x.UserId == userId && x.NewEmail == newEmail)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync();

        if (last != null)
        {
            var seconds = (int)(DateTimeOffset.UtcNow - last.CreatedAt).TotalSeconds;
            if (seconds < 120)
                return BadRequest($"Wait {120 - seconds} seconds");
        }

        var code = CodeGenerator.Generate4Digits();
        var hash = HashService.Sha256($"{userId}:{newEmail}:{code}");

        var entity = new EmailChangeCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NewEmail = newEmail,
            CodeHash = hash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            Attempts = 0,
            IsUsed = false
        };

        _db.EmailChangeCodes.Add(entity);
        await _db.SaveChangesAsync();

        // временно: выводим код в консоль
        Console.WriteLine($"[CHANGE EMAIL CODE] user={userId} => {newEmail} => {code}");

        return Ok(new { cooldownSeconds = 120 });
    }

    [Authorize]
    [HttpPost("confirm-change-email")]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] AuthConfirmChangeEmailRequest request)
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var newEmail = request.NewEmail.Trim().ToLower();
        var code = request.Code.Trim();

        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
            return BadRequest("Invalid email");

        if (code.Length != 4 || !code.All(char.IsDigit))
            return BadRequest("Invalid code");

        var entity = await _db.EmailChangeCodes
            .Where(x => x.UserId == userId && x.NewEmail == newEmail)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity == null)
            return BadRequest("Code not found");

        if (entity.IsUsed)
            return BadRequest("Code already used");

        if (DateTimeOffset.UtcNow > entity.ExpiresAt)
            return BadRequest("Code expired");

        if (entity.Attempts >= 5)
            return BadRequest("Too many attempts");

        entity.Attempts++;

        var inputHash = HashService.Sha256($"{userId}:{newEmail}:{code}");
        if (entity.CodeHash != inputHash)
        {
            await _db.SaveChangesAsync();
            return BadRequest("Invalid code");
        }

        entity.IsUsed = true;

        // проверяем что email не заняли пока мы подтверждали
        var taken = await _db.Users.AnyAsync(x => x.Email == newEmail);
        if (taken)
            return BadRequest("Email already used");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
            return NotFound("User not found");

        user.Email = newEmail;

        await _db.SaveChangesAsync();

        // важно: после смены email лучше выдать новый токен
        var token = _jwt.CreateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Points
            }
        });
    }

    [Authorize]
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var favorites = await _db.UserFavorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Product)
            .Select(f => new
            {
                f.Product.Id,
                f.Product.Name,
                f.Product.Price,
                f.Product.ImageUrl
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [Authorize]
    [HttpPost("favorites/{productId}")]
    public async Task<IActionResult> AddFavorite(Guid productId)
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        if (await _db.UserFavorites.AnyAsync(f => f.UserId == userId && f.ProductId == productId))
            return BadRequest("Already in favorites");

        _db.UserFavorites.Add(new UserFavorite
        {
            UserId = userId,
            ProductId = productId
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize]
    [HttpDelete("favorites/{productId}")]
    public async Task<IActionResult> RemoveFavorite(Guid productId)
    {
        var userIdStr = User.FindFirstValue("uid");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized("Invalid token");

        var favorite = await _db.UserFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        if (favorite == null)
            return NotFound();

        _db.UserFavorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return Ok();
    }
}