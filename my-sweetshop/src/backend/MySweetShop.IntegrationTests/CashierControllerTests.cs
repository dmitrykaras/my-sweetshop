using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySweetshop.Api.Contracts;
using MySweetShop.Api.Contracts;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using System.Net;
using System.Net.Http.Json;
using Xunit;

/// <summary>
/// Тесты рассчитаны на значения LoyaltyOptions по умолчанию:
/// AccrualRate = 0.03, RubPerPoint = 1, MaxRedeemShare = 0.5, CashierSessionMinutes = 5, MaxOrderAmount = 1 000 000.
/// </summary>
public class CashierControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CashierControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private async Task<(Guid CashierId, Guid CustomerId, Guid SessionId)> ArrangeSessionAsync(
        int customerPoints = 0,
        TimeSpan? sessionRemaining = null,
        bool completed = false)
    {
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync(points: customerPoints);
        Guid sessionId = await _factory.SeedSessionAsync(customerId, cashierId, sessionRemaining, completed);
        return (cashierId, customerId, sessionId);
    }

    #region POST /api/cashier/scan

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 200 OK, данные клиента и создаёт кассовую сессию")]
    public async Task Scan_ReturnsSession_WhenTokenIsValid()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync(points: 250, firstName: "Мария");
        var (tokenId, token) = await _factory.SeedQrTokenAsync(customerId);

        var client = _factory.CreateClientFor(cashierId);
        var before = DateTimeOffset.UtcNow;

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        var after = DateTimeOffset.UtcNow;
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ScanResponse>();

        Assert.NotNull(result);
        Assert.Equal(tokenId, result.SessionId);
        Assert.Equal("Мария", result.CustomerName);
        Assert.Equal(250, result.Points);
        Assert.Equal(1m, result.RubPerPoint);
        Assert.Equal(0.5m, result.MaxRedeemShare);
        Assert.InRange(result.SessionExpiresAt, before.AddMinutes(5).AddSeconds(-1), after.AddMinutes(5).AddSeconds(1));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstAsync(t => t.Id == tokenId);

            Assert.True(entity.IsUsed);
            Assert.NotNull(entity.UsedAt);
            Assert.Equal(cashierId, entity.ScannedByCashierId);
            Assert.NotNull(entity.SessionExpiresAt);
            Assert.Null(entity.CompletedAt);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/scan - если у клиента нет имени, возвращает \"Клиент\"")]
    public async Task Scan_ReturnsDefaultCustomerName_WhenFirstNameIsEmpty()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync(firstName: null);
        var (_, token) = await _factory.SeedQrTokenAsync(customerId);

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ScanResponse>();

        Assert.NotNull(result);
        Assert.Equal("Клиент", result.CustomerName);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - обрезает пробелы вокруг токена")]
    public async Task Scan_TrimsToken()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync();
        var (_, token) = await _factory.SeedQrTokenAsync(customerId);

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = $"  {token}  " });

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 401 Unauthorized, если заголовок авторизации не передан")]
    public async Task Scan_ReturnsUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = "any" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 403 Forbidden, если пользователь не кассир")]
    public async Task Scan_Returns403Forbidden_WhenUserIsNotCashier()
    {
        // Arrange
        Guid notCashierId = await _factory.SeedUserAsync(isCashier: false);
        Guid customerId = await _factory.SeedUserAsync();
        var (tokenId, token) = await _factory.SeedQrTokenAsync(customerId);

        var client = _factory.CreateClientFor(notCashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstAsync(t => t.Id == tokenId);
            Assert.False(entity.IsUsed); // токен не сожжён
        }
    }

    [Theory(DisplayName = "POST /api/cashier/scan - возвращает 400 Bad Request, если токен пустой")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Scan_Returns400BadRequest_WhenTokenIsEmpty(string token)
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 400 Bad Request, если токен длиннее 128 символов")]
    public async Task Scan_Returns400BadRequest_WhenTokenIsTooLong()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = new string('a', 129) });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Некорректный QR-код", await response.ReadMessageAsync());
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 400 Bad Request для неизвестного токена")]
    public async Task Scan_Returns400BadRequest_WhenTokenDoesNotExist()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = "MSS1-unknown-token" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 400 Bad Request, если QR просрочен")]
    public async Task Scan_Returns400BadRequest_WhenTokenIsExpired()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync();
        var (tokenId, token) = await _factory.SeedQrTokenAsync(customerId, lifetime: TimeSpan.FromMinutes(-1));

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstAsync(t => t.Id == tokenId);
            Assert.False(entity.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/scan - возвращает 400 Bad Request, если QR уже использован")]
    public async Task Scan_Returns400BadRequest_WhenTokenIsAlreadyUsed()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync();
        var (_, token) = await _factory.SeedQrTokenAsync(customerId, isUsed: true);

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - повторное сканирование того же QR возвращает 400 Bad Request")]
    public async Task Scan_Returns400BadRequest_WhenSameTokenScannedTwice()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync();
        var (_, token) = await _factory.SeedQrTokenAsync(customerId);

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var first = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });
        var second = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/scan - кассир не может отсканировать собственный QR")]
    public async Task Scan_Returns400BadRequest_WhenCashierScansOwnQr()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var (tokenId, token) = await _factory.SeedQrTokenAsync(cashierId);

        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = token });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstAsync(t => t.Id == tokenId);
            Assert.False(entity.IsUsed);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/scan - при одновременном сканировании одного QR успешным будет только один запрос")]
    public async Task Scan_OnlyOneCashierSucceeds_WhenRequestsAreParallel()
    {
        // Arrange
        Guid cashier1 = await _factory.SeedUserAsync(isCashier: true);
        Guid cashier2 = await _factory.SeedUserAsync(isCashier: true);
        Guid customerId = await _factory.SeedUserAsync();
        var (tokenId, token) = await _factory.SeedQrTokenAsync(customerId);

        var clients = new[] { _factory.CreateClientFor(cashier1), _factory.CreateClientFor(cashier2) };

        // Act
        var tasks = Enumerable.Range(0, 10)
            .Select(i => clients[i % 2].PostAsJsonAsync("/api/cashier/scan", new { Token = token }))
            .ToArray();
        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.UserQrTokens.FirstAsync(t => t.Id == tokenId);

            Assert.True(entity.IsUsed);
            Assert.Contains(entity.ScannedByCashierId, new Guid?[] { cashier1, cashier2 });
        }
    }

    #endregion

    #region Проверка роли кассира

    [Fact(DisplayName = "Права кассира проверяются по БД на каждый запрос: после отзыва роли запросы возвращают 403")]
    public async Task CashierRole_IsCheckedAgainstDatabaseOnEveryRequest()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var beforeRevoke = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = "unknown-token" });

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = await db.Users.FirstAsync(u => u.Id == cashierId);
            user.IsCashier = false;
            await db.SaveChangesAsync();
        }

        var afterRevoke = await client.PostAsJsonAsync("/api/cashier/scan", new { Token = "unknown-token" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, beforeRevoke.StatusCode); // роль прошла, токен неверный
        Assert.Equal(HttpStatusCode.Forbidden, afterRevoke.StatusCode);
    }

    #endregion

    #region POST /api/cashier/sessions/{sessionId}/accrue

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - начисляет 3% от суммы заказа и пишет транзакцию")]
    public async Task Accrue_AddsPointsAndCreatesTransaction_WhenSessionIsActive()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 100);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AccrueResponse>();
        Assert.NotNull(result);
        Assert.Equal(30, result.Earned);
        Assert.Equal(130, result.NewBalance);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await db.Users.FirstAsync(u => u.Id == customerId);
            Assert.Equal(130, user.Points);

            var session = await db.UserQrTokens.FirstAsync(t => t.Id == sessionId);
            Assert.NotNull(session.CompletedAt);

            var tx = await db.PointsTransactions.SingleAsync(t => t.QrTokenId == sessionId);
            Assert.Equal(PointsTransactionType.Accrual, tx.Type);
            Assert.Equal(30, tx.Points);
            Assert.Equal(1000m, tx.OrderAmount);
            Assert.Equal(customerId, tx.UserId);
            Assert.Equal(cashierId, tx.CashierId);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - дробная часть баллов отбрасывается вниз")]
    public async Task Accrue_FloorsEarnedPoints()
    {
        // Arrange
        var (cashierId, _, sessionId) = await ArrangeSessionAsync();
        var client = _factory.CreateClientFor(cashierId);

        // Act: 99.99 * 0.03 = 2.9997 -> 2
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 99.99 });

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AccrueResponse>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Earned);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400, если сумма слишком мала для начисления")]
    public async Task Accrue_Returns400BadRequest_WhenAmountIsTooSmall()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 10);
        var client = _factory.CreateClientFor(cashierId);

        // Act: 33 * 0.03 = 0.99 -> 0 баллов
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 33 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.UserQrTokens.FirstAsync(t => t.Id == sessionId);

            Assert.Null(session.CompletedAt); // сессия не потрачена
            Assert.Equal(10, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Theory(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400 Bad Request при некорректной сумме заказа")]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(1000001)]  // больше MaxOrderAmount
    [InlineData(100.555)]  // больше двух знаков после запятой
    public async Task Accrue_Returns400BadRequest_WhenAmountIsInvalid(double amount)
    {
        // Arrange
        var (cashierId, _, sessionId) = await ArrangeSessionAsync();
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = amount });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Некорректная сумма заказа", await response.ReadMessageAsync());
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 401 Unauthorized, если заголовок авторизации не передан")]
    public async Task Accrue_ReturnsUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{Guid.NewGuid()}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 403 Forbidden, если пользователь не кассир")]
    public async Task Accrue_Returns403Forbidden_WhenUserIsNotCashier()
    {
        // Arrange
        var (_, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 100);
        Guid notCashierId = await _factory.SeedUserAsync(isCashier: false);
        var client = _factory.CreateClientFor(notCashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(100, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400, если сессию открыл другой кассир")]
    public async Task Accrue_Returns400BadRequest_WhenSessionBelongsToAnotherCashier()
    {
        // Arrange
        var (_, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 100);
        Guid anotherCashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(anotherCashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(100, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400, если сессия истекла")]
    public async Task Accrue_Returns400BadRequest_WhenSessionIsExpired()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(
            customerPoints: 100, sessionRemaining: TimeSpan.FromMinutes(-1));
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(100, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400, если сессия уже завершена")]
    public async Task Accrue_Returns400BadRequest_WhenSessionIsAlreadyCompleted()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 100, completed: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(100, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 400 для несуществующей сессии")]
    public async Task Accrue_Returns400BadRequest_WhenSessionDoesNotExist()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{Guid.NewGuid()}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - возвращает 404, если id сессии не является GUID")]
    public async Task Accrue_Returns404NotFound_WhenSessionIdIsNotGuid()
    {
        // Arrange
        Guid cashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync("/api/cashier/sessions/not-a-guid/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - повторный запрос по той же сессии возвращает 400, баллы начислены один раз")]
    public async Task Accrue_Returns400BadRequest_WhenCalledTwiceForSameSession()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 0);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var first = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });
        var second = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 });

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(30, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/accrue - параллельные запросы (двойной тап) начисляют баллы ровно один раз")]
    public async Task Accrue_CreditsOnlyOnce_WhenRequestsAreParallel()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 0);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/accrue", new { OrderAmount = 1000 }))
            .ToArray();
        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(30, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
            Assert.Equal(1, await db.PointsTransactions.CountAsync(t => t.QrTokenId == sessionId));
        }
    }

    #endregion

    #region POST /api/cashier/sessions/{sessionId}/redeem

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - списывает баллы, возвращает итоговую сумму и пишет транзакцию")]
    public async Task Redeem_DebitsPointsAndReturnsFinalAmount_WhenDataIsValid()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RedeemResponse>();
        Assert.NotNull(result);
        Assert.Equal(900m, result.FinalAmount);
        Assert.Equal(100, result.Redeemed);
        Assert.Equal(400, result.NewBalance);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(400, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
            Assert.NotNull((await db.UserQrTokens.FirstAsync(t => t.Id == sessionId)).CompletedAt);

            var tx = await db.PointsTransactions.SingleAsync(t => t.QrTokenId == sessionId);
            Assert.Equal(PointsTransactionType.Redemption, tx.Type);
            Assert.Equal(-100, tx.Points);
            Assert.Equal(1000m, tx.OrderAmount);
            Assert.Equal(customerId, tx.UserId);
            Assert.Equal(cashierId, tx.CashierId);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - можно списать ровно 50% от суммы заказа")]
    public async Task Redeem_Succeeds_WhenRedeemingExactlyMaxShare()
    {
        // Arrange
        var (cashierId, _, sessionId) = await ArrangeSessionAsync(customerPoints: 1000);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 500 });

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RedeemResponse>();

        Assert.NotNull(result);
        Assert.Equal(500m, result.FinalAmount);
        Assert.Equal(500, result.NewBalance);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - итоговая сумма округляется до копеек")]
    public async Task Redeem_RoundsFinalAmountToTwoDecimals()
    {
        // Arrange
        var (cashierId, _, sessionId) = await ArrangeSessionAsync(customerPoints: 100);
        var client = _factory.CreateClientFor(cashierId);

        // Act: 99.99 - 10 = 89.99
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 99.99, PointsToRedeem = 10 });

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RedeemResponse>();

        Assert.NotNull(result);
        Assert.Equal(89.99m, result.FinalAmount);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400, если списание больше 50% от суммы заказа")]
    public async Task Redeem_Returns400BadRequest_WhenRedeemExceedsMaxShare()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 1000);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 501 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("500", await response.ReadMessageAsync());

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(1000, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
            Assert.Null((await db.UserQrTokens.FirstAsync(t => t.Id == sessionId)).CompletedAt);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400 при нехватке баллов, баланс и сессия не меняются")]
    public async Task Redeem_Returns400BadRequest_WhenCustomerHasNotEnoughPoints()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 50);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("У клиента недостаточно баллов", await response.ReadMessageAsync());

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(50, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
            // транзакция откатилась: кассир может повторить операцию с другим количеством баллов
            Assert.Null((await db.UserQrTokens.FirstAsync(t => t.Id == sessionId)).CompletedAt);
            Assert.Equal(0, await db.PointsTransactions.CountAsync(t => t.QrTokenId == sessionId));
        }
    }

    [Theory(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400, если количество баллов не положительное")]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Redeem_Returns400BadRequest_WhenPointsToRedeemIsNotPositive(int points)
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = points });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(500, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Theory(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400 Bad Request при некорректной сумме заказа")]
    [InlineData(0)]
    [InlineData(-100)]
    [InlineData(1000001)]
    [InlineData(100.555)]
    public async Task Redeem_Returns400BadRequest_WhenAmountIsInvalid(double amount)
    {
        // Arrange
        var (cashierId, _, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = amount, PointsToRedeem = 10 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Некорректная сумма заказа", await response.ReadMessageAsync());
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 401 Unauthorized, если заголовок авторизации не передан")]
    public async Task Redeem_ReturnsUnauthorized_WhenUserIsNotAuthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{Guid.NewGuid()}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 403 Forbidden, если пользователь не кассир")]
    public async Task Redeem_Returns403Forbidden_WhenUserIsNotCashier()
    {
        // Arrange
        var (_, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        Guid notCashierId = await _factory.SeedUserAsync(isCashier: false);
        var client = _factory.CreateClientFor(notCashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(500, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400, если сессию открыл другой кассир")]
    public async Task Redeem_Returns400BadRequest_WhenSessionBelongsToAnotherCashier()
    {
        // Arrange
        var (_, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        Guid anotherCashierId = await _factory.SeedUserAsync(isCashier: true);
        var client = _factory.CreateClientFor(anotherCashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(500, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - возвращает 400, если сессия истекла")]
    public async Task Redeem_Returns400BadRequest_WhenSessionIsExpired()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(
            customerPoints: 500, sessionRemaining: TimeSpan.FromMinutes(-1));
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var response = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(500, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - повторный запрос по той же сессии возвращает 400, списано один раз")]
    public async Task Redeem_Returns400BadRequest_WhenCalledTwiceForSameSession()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var first = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });
        var second = await client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
            new { OrderAmount = 1000, PointsToRedeem = 100 });

        // Assert
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(400, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
        }
    }

    [Fact(DisplayName = "POST /api/cashier/sessions/{id}/redeem - параллельные запросы (двойной тап) списывают баллы ровно один раз")]
    public async Task Redeem_DebitsOnlyOnce_WhenRequestsAreParallel()
    {
        // Arrange
        var (cashierId, customerId, sessionId) = await ArrangeSessionAsync(customerPoints: 500);
        var client = _factory.CreateClientFor(cashierId);

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => client.PostAsJsonAsync($"/api/cashier/sessions/{sessionId}/redeem",
                new { OrderAmount = 1000, PointsToRedeem = 100 }))
            .ToArray();
        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Assert.Equal(400, (await db.Users.FirstAsync(u => u.Id == customerId)).Points);
            Assert.Equal(1, await db.PointsTransactions.CountAsync(t => t.QrTokenId == sessionId));
        }
    }

    #endregion
}