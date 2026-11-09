using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetshop.Api.Contracts;
using MySweetShop.Api.Contracts;
using MySweetShop.Api.Data;
using MySweetShop.Api.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class QrControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public QrControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    #region POST /api/qr/generate

    [Fact(DisplayName = "POST /api/qr/generate - возвращает 200 OK и токен со сроком жизни ~120 секунд")]
    public async Task Generate_ReturnsToken_WhenUserIsAuthorized()
    {
        // Arrange
        Guid userId = await _factory.SeedUserAsync();
        var client = _factory.CreateClientFor(userId);

        var before = DateTimeOffset.UtcNow;

        // Act
        var response = await client.PostAsync("/api/qr/generate", null);

        // Assert
        var after = DateTimeOffset.UtcNow;
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<QrTokenResponse>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal(43, result.Token.Length); // 256 бит в URL-safe base64
        Assert.InRange(result.ExpiresAt, before.AddSeconds(119), after.AddSeconds(121));
    }

    [Fact(DisplayName = "POST /api/qr/generate - в БД сохраняется только хэш токена, а не сам токен")]
    public async Task Generate_StoresOnlyTokenHash()
    {
        // Arrange
        Guid userId = await _factory.SeedUserAsync();
        var client = _factory.CreateClientFor(userId);

        // Act
        var response = await client.PostAsync("/api/qr/generate", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QrTokenResponse>();
        Assert.NotNull(result);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstOrDefaultAsync(t => t.UserId == userId);

            Assert.NotNull(entity);
            Assert.Equal(QrTokenHelper.Hash(result.Token), entity.TokenHash);
            Assert.NotEqual(result.Token, entity.TokenHash);
            Assert.False(entity.IsUsed);
            Assert.Null(entity.ScannedByCashierId);
            Assert.Null(entity.CompletedAt);
        }
    }

    [Fact(DisplayName = "POST /api/qr/generate - возвращает 401 Unauthorized, если заголовок авторизации не передан")]
    public async Task Generate_ReturnsUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/qr/generate", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/qr/generate - повторный запрос гасит прежний QR, активным остаётся только один")]
    public async Task Generate_InvalidatesPreviousToken_WhenCalledAgain()
    {
        // Arrange
        Guid userId = await _factory.SeedUserAsync();
        var client = _factory.CreateClientFor(userId);

        // Act
        var firstResponse = await client.PostAsync("/api/qr/generate", null);
        var secondResponse = await client.PostAsync("/api/qr/generate", null);

        // Assert
        firstResponse.EnsureSuccessStatusCode();
        secondResponse.EnsureSuccessStatusCode();

        var first = await firstResponse.Content.ReadFromJsonAsync<QrTokenResponse>();
        var second = await secondResponse.Content.ReadFromJsonAsync<QrTokenResponse>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotEqual(first.Token, second.Token);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tokens = await db.UserQrTokens.Where(t => t.UserId == userId).ToListAsync();
            var now = DateTimeOffset.UtcNow;

            Assert.Equal(2, tokens.Count);

            var active = tokens.Where(t => !t.IsUsed && t.ExpiresAt > now).ToList();
            Assert.Single(active);
            Assert.Equal(QrTokenHelper.Hash(second.Token), active[0].TokenHash);
        }
    }

    [Fact(DisplayName = "POST /api/qr/generate - не трогает QR других клиентов")]
    public async Task Generate_DoesNotInvalidateTokensOfOtherUsers()
    {
        // Arrange
        Guid userId = await _factory.SeedUserAsync();
        Guid otherUserId = await _factory.SeedUserAsync();
        var (otherTokenId, _) = await _factory.SeedQrTokenAsync(otherUserId);

        var client = _factory.CreateClientFor(userId);

        // Act
        var response = await client.PostAsync("/api/qr/generate", null);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var otherToken = await db.UserQrTokens.FirstAsync(t => t.Id == otherTokenId);

            Assert.True(otherToken.ExpiresAt > DateTimeOffset.UtcNow);
            Assert.False(otherToken.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /api/qr/generate - не меняет уже использованный (отсканированный) токен")]
    public async Task Generate_DoesNotTouchAlreadyUsedToken()
    {
        // Arrange
        Guid userId = await _factory.SeedUserAsync();
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid sessionId = await _factory.SeedSessionAsync(userId, cashierId);

        DateTimeOffset expiresBefore;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            expiresBefore = (await db.UserQrTokens.FirstAsync(t => t.Id == sessionId)).ExpiresAt;
        }

        var client = _factory.CreateClientFor(userId);

        // Act
        var response = await client.PostAsync("/api/qr/generate", null);

        // Assert
        response.EnsureSuccessStatusCode();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.UserQrTokens.FirstAsync(t => t.Id == sessionId);

            Assert.True(session.IsUsed);
            Assert.Equal(expiresBefore, session.ExpiresAt);
        }
    }

    #endregion
}