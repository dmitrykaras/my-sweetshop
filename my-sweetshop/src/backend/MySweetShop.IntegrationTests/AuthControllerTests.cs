using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Services;
using Xunit;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region POST /auth/request-code

    [Fact(DisplayName = "POST /auth/request-code - успешно создает код верификации и возвращает cooldown")]
    public async Task RequestCode_CreatesVerificationCode_WhenEmailIsValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        string email = $"auth_req_{Guid.NewGuid()}@sweetshop.test";

        var request = new { Email = email };

        // Act
        var response = await client.PostAsJsonAsync("/auth/request-code", request);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var codeEntity = await db.EmailVerificationCodes
                .FirstOrDefaultAsync(x => x.Email == email);

            Assert.NotNull(codeEntity);
            Assert.False(codeEntity.IsUsed);
            Assert.Equal(0, codeEntity.Attempts);
        }
    }

    [Fact(DisplayName = "POST /auth/request-code - возвращает 400 Bad Request при невалидном формате Email")]
    public async Task RequestCode_Returns400BadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Email = "invalid-email-without-at" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/request-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /auth/request-code - возвращает 400 Bad Request при попытке повторной запроса в период cooldown (<120 сек)")]
    public async Task RequestCode_Returns400BadRequest_WhenCooldownIsActive()
    {
        // Arrange
        string email = $"cooldown_auth_{Guid.NewGuid()}@sweetshop.test";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                CodeHash = "hash",
                CreatedAt = DateTimeOffset.UtcNow.AddSeconds(-30), // 30 секунд назад
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var request = new { Email = email };

        // Act
        var response = await client.PostAsJsonAsync("/auth/request-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region POST /auth/verify-code

    [Fact(DisplayName = "POST /auth/verify-code - успешно создает нового пользователя и возвращает токены при правильном коде")]
    public async Task VerifyCode_CreatesNewUserAndReturnsTokens_WhenCodeIsValid()
    {
        // Arrange
        string email = $"verify_new_{Guid.NewGuid()}@sweetshop.test";
        string code = "5555";
        string hash = HashService.Sha256($"{email}:{code}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var verifyRequest = new { Email = email, Code = code };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-code", verifyRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthVerifyResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.True(result.NeedsProfile);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var refreshToken = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == result.RefreshToken);

            Assert.NotNull(user);
            Assert.NotNull(refreshToken);
            Assert.Equal(user.Id, refreshToken.UserId);
        }
    }

    [Fact(DisplayName = "POST /auth/verify-code - возвращает 400 Bad Request при неверном формате кода (не 4 цифры)")]
    public async Task VerifyCode_Returns400BadRequest_WhenCodeFormatIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { Email = "user@test.com", Code = "123" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /auth/verify-code - уменьшает число попыток при вводе неверного кода")]
    public async Task VerifyCode_DecrementsAttempts_WhenCodeIsIncorrect()
    {
        // Arrange
        string email = $"wrong_code_{Guid.NewGuid()}@sweetshop.test";
        string correctCode = "1234";
        string hash = HashService.Sha256($"{email}:{correctCode}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 0,
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var request = new { Email = email, Code = "9999" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var codeEntity = await db.EmailVerificationCodes.FirstAsync(x => x.Email == email);

            Assert.Equal(1, codeEntity.Attempts);
        }
    }

    [Fact(DisplayName = "POST /auth/verify-code - блокирует попытки при превышении лимита (5 попыток)")]
    public async Task VerifyCode_BlocksUser_WhenMaxAttemptsExceeded()
    {
        // Arrange
        string email = $"blocked_{Guid.NewGuid()}@sweetshop.test";
        string correctCode = "1234";
        string hash = HashService.Sha256($"{email}:{correctCode}");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.EmailVerificationCodes.Add(new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                Email = email,
                CodeHash = hash,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
                Attempts = 4, // 5-я попытка заблокирует
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var request = new { Email = email, Code = "0000" };

        // Act
        var response = await client.PostAsJsonAsync("/auth/verify-code", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var codeEntity = await db.EmailVerificationCodes.FirstAsync(x => x.Email == email);

            Assert.Equal(5, codeEntity.Attempts);
            Assert.NotNull(codeEntity.BlockedUntil);
        }
    }

    #endregion

    #region POST /auth/refresh

    [Fact(DisplayName = "POST /auth/refresh - успешно обновляет пары JWT и Refresh Token при валидном токене")]
    public async Task RefreshToken_ReturnsNewTokenPair_WhenRefreshTokenIsValid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string oldRefreshTokenValue = Guid.NewGuid().ToString("N");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new User
            {
                Id = userId,
                Email = $"refresh_user_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Users.Add(user);

            db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = oldRefreshTokenValue,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsUsed = false
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var refreshRequest = new { RefreshToken = oldRefreshTokenValue };

        // Act
        var response = await client.PostAsJsonAsync("/auth/refresh", refreshRequest);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RefreshResponse>();

        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.NotNull(result.RefreshToken);
        Assert.NotEqual(oldRefreshTokenValue, result.RefreshToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var oldTokenEntity = await db.RefreshTokens.FirstAsync(r => r.Token == oldRefreshTokenValue);
            var newTokenEntity = await db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == result.RefreshToken);

            Assert.True(oldTokenEntity.IsUsed);
            Assert.NotNull(newTokenEntity);
            Assert.False(newTokenEntity.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /auth/refresh - возвращает 401 Unauthorized, если токен уже использован")]
    public async Task RefreshToken_Returns401Unauthorized_WhenTokenIsAlreadyUsed()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string usedRefreshToken = Guid.NewGuid().ToString("N");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Users.Add(new User
            {
                Id = userId,
                Email = $"used_token_{userId}@sweetshop.test",
                CreatedAt = DateTimeOffset.UtcNow
            });

            db.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = usedRefreshToken,
                UserId = userId,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsUsed = true
            });

            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var refreshRequest = new { RefreshToken = usedRefreshToken };

        // Act
        var response = await client.PostAsJsonAsync("/auth/refresh", refreshRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    private record AuthVerifyResponse(string? Token, string RefreshToken, UserDto User, bool NeedsProfile);
    private record UserDto(Guid Id, string Email, string? FirstName, string? LastName, int Points);
    private record RefreshResponse(string Token, string RefreshToken);
}