using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Contracts;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;
using MyCandyShop.Api.Services;
using System.Security.Claims;

namespace MyCandyShop.Api.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IObjectStorage _storage;
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public ProfileController(AppDbContext db, JwtService jwt, IObjectStorage storage)
        {
            _db = db;
            _jwt = jwt;
            _storage = storage;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        // Метод для просмотра баллов в профиле
        [Authorize]
        [HttpGet("points")]
        public async Task<IActionResult> GetPoints()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            return Ok(new
            {
                user.Points
            });
        }

        // Метод для удаления избранных продуктов
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

        // Метод для добавления израбнных продуктов
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

        // Метод для получения израбнные продуктов
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
                    f.Product.Description,
                    f.Product.Price,
                    ImageUrl = string.IsNullOrEmpty(f.Product.ImageKey)
                   ? null
                   : _storage.GetPreSignedUrl(f.Product.ImageKey, TimeSpan.FromHours(6)),
                    CategoryId = f.Product.CategoryId
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // Метод для подтверждения смены почты
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
                return BadRequest(new ApiErrorResponse { Message = "Code not found" });

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

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = Guid.NewGuid().ToString("N"), // Случайная уникальная строка
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // Срок жизни 30 дней
                IsUsed = false
            };

            _db.RefreshTokens.Add(refreshTokenEntity);

            await _db.SaveChangesAsync();

            // важно: после смены email лучше выдать новый токен
            var token = _jwt.CreateToken(user);

            return Ok(new
            {
                token,
                refreshToken = refreshTokenEntity.Token,
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

        // Метод для запроса смены почты
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

        private string? NormalizeField(string? field) =>
            string.IsNullOrWhiteSpace(field) ? null : field.Trim();

        // Метод для смены имени и/или фамилии
        [HttpPatch]
        public async Task<IActionResult> PatchProfile([FromBody] PatchProfileRequest request)
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");

            // обновляем только те поля, что пришли
            var firstName = NormalizeField(request.FirstName);
            var lastName = NormalizeField(request.LastName);

            if (firstName != null) user.FirstName = firstName;
            if (lastName != null) user.LastName = lastName;


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

        // Метод для просмотра текущего профиля
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

        // Метод для смены и имени и фамилии
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

        // Метод для проверки кода (не создаёт новых пользователей)
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] AuthVerifyCodeRequest request)
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

            if (entity.IsUsed )
                return BadRequest(new ApiErrorResponse { Error = "code_already_used", Message = "Code already used" });

            if (DateTimeOffset.UtcNow > entity.ExpiresAt)
                return BadRequest(new ApiErrorResponse { Error = "code_expired", Message = "Code expired" });

            // Проверка блокировки и попыток
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
                }
                await _db.SaveChangesAsync();

                return BadRequest(new ApiErrorResponse
                {
                    Error = "invalid_code",
                    Message = "Invalid code",
                    AttemptsLeft = Math.Max(0, maxAttempts - entity.Attempts)
                });
            }

            // Код верный
            entity.IsUsed = true;
            await _db.SaveChangesAsync();

            return Ok(new { success = true });
        }

    }
}