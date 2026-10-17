using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySweetShop.Api.Contracts;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Services;
using System.Security.Claims;

namespace MySweetShop.Api.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController(AppDbContext db, JwtService jwt) : ControllerBase
    {
        private readonly AppDbContext _db = db;
        private readonly JwtService _jwt = jwt;

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

        // Метод для запроса кода для смены почты
        [Authorize]
        [HttpPost("request-code-for-change-email")]
        public async Task<IActionResult> RequestChangeEmail([FromBody] AuthRequestChangeEmailRequest request)
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            // Нормализуем новую почту: именно её проверяем, сохраняем и ищем дальше
            var newEmail = request.NewEmail.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains('@'))
                return BadRequest("Invalid email");

            // Текущую почту берём из БД, а не из тела запроса
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Unauthorized("Invalid token");

            if (newEmail == user.Email)
                return BadRequest("New email must differ from the current one");

            // проверка на то, что Email уже занят
            if (await _db.Users.AnyAsync(x => x.Email == newEmail))
                return BadRequest("Email already used by another account");

            // cooldown 2 минуты — на пользователя (а не на конкретный адрес),
            var last = await _db.EmailChangeCodes
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (last != null)
            {
                var seconds = (int)(DateTimeOffset.UtcNow - last.CreatedAt).TotalSeconds;
                if (seconds < 120)
                    return BadRequest($"Wait {120 - seconds} seconds");
            }

            var code = CodeGenerator.Generate6Digits();
            var hash = HashService.Sha256($"{userId}:{newEmail}:{code}");

            var entity = new EmailChangeCode
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = user.Email,
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

        // Метод для проверки кода и смены почты
        [Authorize]
        [HttpPost("verify-change-email")]
        public async Task<IActionResult> VerifyChangeEmail([FromBody] AuthConfirmChangeEmailRequest request)
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var newEmail = request.NewEmail.Trim().ToLower();
            var code = request.Code.Trim();

            // Ровно 6 символов и только ASCII-цифры
            if (code.Length != 6 || !code.All(char.IsAsciiDigit))
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

            const int maxAttempts = 5;
            const int blockSeconds = 120;

            if (entity.BlockedUntil.HasValue && DateTimeOffset.UtcNow < entity.BlockedUntil.Value)
            {
                var retry = (int)Math.Ceiling((entity.BlockedUntil.Value - DateTimeOffset.UtcNow).TotalSeconds);
                return BadRequest(new ApiErrorResponse
                {
                    Error = "too_many_attempts",
                    Message = "Too many attempts. Cooldown active.",
                    AttemptsLeft = 0,
                    RetryAfterSeconds = retry
                });
            }

            // Атомарно "занимаем" попытку одним UPDATE прямо в БД.
            // Параллельные запросы не смогут обойти лимит: каждый видит актуальный счётчик.
            var reserved = await _db.EmailChangeCodes
                .Where(x => x.Id == entity.Id && !x.IsUsed && x.Attempts < maxAttempts)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Attempts, x => x.Attempts + 1));

            if (reserved == 0)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Error = "too_many_attempts",
                    Message = "Too many failed attempts. Code invalidated.",
                    AttemptsLeft = 0,
                    RetryAfterSeconds = blockSeconds
                });
            }

            var inputHash = HashService.Sha256($"{userId}:{newEmail}:{code}");
            if (entity.CodeHash != inputHash)
            {
                // Читаем актуальный счётчик (его могли увеличить параллельные запросы)
                var attemptsUsed = await _db.EmailChangeCodes
                    .AsNoTracking()
                    .Where(x => x.Id == entity.Id)
                    .Select(x => x.Attempts)
                    .FirstAsync();

                if (attemptsUsed >= maxAttempts)
                {
                    // Аннулируем код и ставим кулдаун
                    await _db.EmailChangeCodes
                        .Where(x => x.Id == entity.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.IsUsed, true)
                            .SetProperty(x => x.BlockedUntil, (DateTimeOffset?)DateTimeOffset.UtcNow.AddSeconds(blockSeconds)));

                    return BadRequest(new ApiErrorResponse
                    {
                        Error = "too_many_attempts",
                        Message = "Too many failed attempts. Code invalidated.",
                        AttemptsLeft = 0,
                        RetryAfterSeconds = blockSeconds
                    });
                }

                return BadRequest(new ApiErrorResponse
                {
                    Error = "invalid_code",
                    Message = "Invalid code",
                    AttemptsLeft = maxAttempts - attemptsUsed
                });
            }

            // Код верный: атомарно гасим его (двойное использование при параллельных запросах невозможно)
            var consumed = await _db.EmailChangeCodes
                .Where(x => x.Id == entity.Id && !x.IsUsed)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true));

            if (consumed == 0)
                return BadRequest("Code already used");

            // Проверяем что email не заняли пока мы подтверждали
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
        [HttpPatch("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            // Получаем ID пользователя из токена
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            // Ищем пользователя
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return NotFound("Пользователь не найден");

            // Кулдаун 5 минут
            if (user.LastProfileUpdate.HasValue)
            {
                var secondsPassed = (int)(DateTimeOffset.UtcNow - user.LastProfileUpdate.Value).TotalSeconds;
                int limit = 300; // 5 минут в секундах

                if (secondsPassed < limit)
                {
                    return BadRequest(new
                    {
                        message = $"Изменять профиль можно раз в 5 минут",
                        retryAfter = limit - secondsPassed
                    });
                }
            }

            // Обновляем только те поля, которые ПРИШЛИ и не пустые
            bool isChanged = false;

            if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Length >= 2)
            {
                user.FirstName = request.FirstName.Trim();
                isChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName.Length >= 2)
            {
                user.LastName = request.LastName.Trim();
                isChanged = true;
            }

            // Сохраняем, только если были реальные изменения
            if (isChanged)
            {
                user.LastProfileUpdate = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                nextUpdateAvailable = user.LastProfileUpdate?.AddMinutes(5)
            });
        }
    }
}