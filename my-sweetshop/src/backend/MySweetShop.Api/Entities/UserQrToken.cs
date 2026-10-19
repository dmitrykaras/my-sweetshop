namespace MySweetshop.Api.Entities;

/// <summary>
/// Одноразовый QR-токен клиента. Одновременно служит "кассовой сессией":
/// после скана (IsUsed = true) кассир 5 минут может провести ОДНУ операцию (CompletedAt).
/// В БД хранится только SHA-256 хэш токена — утечка БД не даёт рабочих QR-кодов.
/// </summary>
public class UserQrToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; } // срок жизни самого QR (120 c)
    public bool IsUsed { get; set; } // QR уже отсканирован
    public DateTimeOffset? UsedAt { get; set; }
    public Guid? ScannedByCashierId { get; set; }
    public DateTimeOffset? SessionExpiresAt { get; set; } // сколько у кассира есть времени на операцию
    public DateTimeOffset? CompletedAt { get; set; } // операция проведена (начисление/списание)
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

}