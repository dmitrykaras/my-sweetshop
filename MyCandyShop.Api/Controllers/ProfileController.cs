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

        // Метод для запроса кода для смены почты
        [Authorize]
        [HttpPost("request-code-for-change-email")]
        public async Task<IActionResult> RequestChangeEmail([FromBody] AuthRequestChangeEmailRequest request)
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var Email = request.Email.Trim().ToLower();
            var newEmail = request.NewEmail.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
                return BadRequest("Invalid email");

            // проверка на то, что Email уже занят 
            if (await _db.Users.AnyAsync(x => x.Email == newEmail))
                return BadRequest("Email already used by another account");

            //cooldown (например 2 минуты на один email)
            var last = await _db.EmailChangeCodes
                .Where(x => x.UserId == userId && x.NewEmail == Email)
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
                Email = request.Email,
                NewEmail = request.NewEmail,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            };

            _db.EmailChangeCodes.Add(entity);
            await _db.SaveChangesAsync();

            // временно: выводим код в консоль
            Console.WriteLine($"[CHANGE EMAIL CODE] user={userId} => {Email} => {code}");

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

            // Проверка на 4 символа и это цифры
            if (code.Length != 4 || !code.All(char.IsDigit))
                return BadRequest("Invalid code");

            var entity = await _db.EmailChangeCodes
                .Where(x => x.UserId == userId && x.NewEmail == newEmail)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            // Проверка наден ли код
            if (entity == null)
                return BadRequest(new ApiErrorResponse { Message = "Code not found" });

            // Проверка на то, что он использован уже
            if (entity.IsUsed)
                return BadRequest("Code already used");

            // Проверка на срок дейсвтия кода
            if (DateTimeOffset.UtcNow > entity.ExpiresAt)
                return BadRequest("Code expired");

            // Проверка, что попыток было меньше 5
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
            // 1. Получаем ID пользователя из токена
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            // 2. Ищем пользователя
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

            // 3. Обновляем только те поля, которые ПРИШЛИ и не пустые
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

            // 4. Сохраняем, только если были реальные изменения
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