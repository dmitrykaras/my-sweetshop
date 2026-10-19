namespace MySweetShop.Api.Entities;

public enum PointsTransactionType
{
    Accrual = 1, // Начисление
    Redemption = 2 // Списание
}

/// <summary> Журнал всех движений баллов (аудит, споры, отчёты) </summary>
public class PointsTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CashierId { get; set; }
    public Guid QrTokenId { get; set; }
    public PointsTransactionType Type { get; set; }
    public decimal OrderAmount { get; set; }
    /// <summary> +N при начислении, -N при списании </summary>
    public int Points { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}