using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Contracts;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.Services;
using System.Linq;
using System.Security.Claims;

namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly IObjectStorage _storage;

    public AuthController(AppDbContext db, JwtService jwt, IObjectStorage storage)
    {
        _db = db;
        _jwt = jwt;
        _storage = storage;
    }

    // Метод для отправки кода
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

    // Метод для верификации кода
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] AuthVerifyCodeRequest request)
    {
        var email = request.Email?.Trim().ToLower();
        var code = request.Code?.Trim();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            return BadRequest(new ApiErrorResponse { Error = "invalid_email", Message = "Invalid email" });

        if (string.IsNullOrWhiteSpace(code) || code.Length != 4 || !code.All(char.IsDigit))
            return BadRequest(new ApiErrorResponse { Error = "invalid_code_format", Message = "Invalid code format" });

        var entity = await _db.EmailVerificationCodes
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (entity == null)
            return BadRequest(new ApiErrorResponse { Error = "code_not_found", Message = "Code not found" });

        if (entity.IsUsed)
            return BadRequest(new ApiErrorResponse { Error = "code_already_used", Message = "Code already used" });

        if (DateTimeOffset.UtcNow > entity.ExpiresAt)
            return BadRequest(new ApiErrorResponse { Error = "code_expired", Message = "Code expired" });

        const int maxAttempts = 5;
        const int blockSeconds = 120;

        if (entity.BlockedUntil.HasValue && DateTimeOffset.UtcNow < entity.BlockedUntil.Value)
        {
            var retry = (int)(entity.BlockedUntil.Value - DateTimeOffset.UtcNow).TotalSeconds;
            return BadRequest(new ApiErrorResponse { Error = "too_many_attempts", Message = "Too many attempts", AttemptsLeft = 0, RetryAfterSeconds = retry });
        }

        entity.Attempts++;
        var inputHash = HashService.Sha256($"{email}:{code}");

        if (entity.CodeHash != inputHash)
        {
            if (entity.Attempts >= maxAttempts)
            {
                entity.BlockedUntil = DateTimeOffset.UtcNow.AddSeconds(blockSeconds);
                await _db.SaveChangesAsync();
                return BadRequest(new ApiErrorResponse { Error = "too_many_attempts", Message = "Too many attempts", AttemptsLeft = 0, RetryAfterSeconds = blockSeconds });
            }

            await _db.SaveChangesAsync();
            return BadRequest(new ApiErrorResponse { Error = "invalid_code", Message = "Invalid code", AttemptsLeft = maxAttempts - entity.Attempts });
        }

        // Код верный
        entity.IsUsed = true;
        await _db.SaveChangesAsync();

        // Получаем текущего пользователя через Claims
        var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        User? user = null;

        if (!string.IsNullOrEmpty(userIdClaim))
        {
            // Залогинен: обновляем почту
            var currentUserId = Guid.Parse(userIdClaim);
            user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return BadRequest(new ApiErrorResponse { Error = "user_not_found", Message = "User not found" });

            if (user.Email != email)
            {
                user.Email = email;
                await _db.SaveChangesAsync();
            }
        }
        else
        {
            // Неавторизованный: проверяем email и создаём при необходимости
            user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
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
        }

        // 1. ГЕНЕРИРУЕМ REFRESH TOKEN (этого у тебя не было!)
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsUsed = false
        };

        // 2. СОХРАНЯЕМ В БАЗУ
        _db.RefreshTokens.Add(refreshTokenEntity);
        await _db.SaveChangesAsync();

        // 3. ФОРМИРУЕМ JWT
        var token = string.IsNullOrEmpty(userIdClaim) ? _jwt.CreateToken(user) : null;
        var needsProfile = string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName);

        // 4. ВОЗВРАЩАЕМ ВСЁ КЛИЕНТУ
        return Ok(new
        {
            token,
            refreshToken = refreshTokenEntity.Token, // Обязательно добавляем это поле!
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

    // Получение нового JWT по refresh token
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest req)
    {
        var oldRefresh = await _db.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == req.RefreshToken);

        if (oldRefresh == null || oldRefresh.IsUsed || oldRefresh.ExpiresAt < DateTime.UtcNow)
            return Unauthorized();

        // 1. Помечаем старый как использованный
        oldRefresh.IsUsed = true;

        // 2. Создаем НОВЫЙ Refresh Token (Rotation)
        var newRefresh = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = oldRefresh.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsUsed = false
        };

        // 3. Генерируем новый JWT
        var newJwt = _jwt.CreateToken(oldRefresh.User);

        _db.RefreshTokens.Add(newRefresh);
        await _db.SaveChangesAsync();

        // Возвращаем ПАРУ
        return Ok(new
        {
            token = newJwt,
            refreshToken = newRefresh.Token
        });
    }
}