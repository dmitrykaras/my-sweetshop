using Microsoft.Extensions.DependencyInjection;
using MySweetshop.Api.Entities;
using MySweetShop.Api.Data;
using MySweetShop.Api.Entities;
using MySweetShop.Api.Services;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>Общие помощники для тестов QR / кассира, чтобы не дублировать Arrange в каждом тесте.</summary>
public static class LoyaltyTestHelpers
{
    public static HttpClient CreateClientFor(this CustomWebApplicationFactory<Program> factory, Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        return client;
    }

    public static async Task<Guid> SeedUserAsync(
        this CustomWebApplicationFactory<Program> factory,
        bool isCashier = false,
        int points = 0,
        string? firstName = "Иван")
    {
        Guid userId = Guid.NewGuid();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Users.Add(new User
        {
            Id = userId,
            Email = $"loyalty_{userId}@sweetshop.test",
            FirstName = firstName,
            Points = points,
            IsCashier = isCashier,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        return userId;
    }

    /// <summary>Свежий (ещё не отсканированный) QR клиента. Возвращает id записи и "сырой" токен.</summary>
    public static async Task<(Guid Id, string Token)> SeedQrTokenAsync(
        this CustomWebApplicationFactory<Program> factory,
        Guid userId,
        TimeSpan? lifetime = null,
        bool isUsed = false)
    {
        string token = QrTokenHelper.NewToken();
        Guid id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.UserQrTokens.Add(new UserQrToken
        {
            Id = id,
            UserId = userId,
            TokenHash = QrTokenHelper.Hash(token),
            CreatedAt = now,
            ExpiresAt = now + (lifetime ?? TimeSpan.FromSeconds(120)),
            IsUsed = isUsed
        });
        await db.SaveChangesAsync();

        return (id, token);
    }

    /// <summary>Уже отсканированный QR = открытая кассовая сессия кассира.</summary>
    public static async Task<Guid> SeedSessionAsync(
        this CustomWebApplicationFactory<Program> factory,
        Guid customerId,
        Guid cashierId,
        TimeSpan? remaining = null,
        bool completed = false)
    {
        Guid id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.UserQrTokens.Add(new UserQrToken
        {
            Id = id,
            UserId = customerId,
            TokenHash = QrTokenHelper.Hash(QrTokenHelper.NewToken()),
            CreatedAt = now.AddMinutes(-1),
            ExpiresAt = now.AddSeconds(60),
            IsUsed = true,
            UsedAt = now.AddSeconds(-30),
            ScannedByCashierId = cashierId,
            SessionExpiresAt = now + (remaining ?? TimeSpan.FromMinutes(5)),
            CompletedAt = completed ? now.AddSeconds(-10) : null
        });
        await db.SaveChangesAsync();

        return id;
    }

    public static async Task<string?> ReadMessageAsync(this HttpResponseMessage response)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.TryGetProperty("message", out var m) ? m.GetString() : null;
    }
}