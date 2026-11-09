namespace MySweetShop.Api.Options;

public class LoyaltyOptions
{
    public int QrLifetimeSeconds { get; set; } = 120;
    public int CashierSessionMinutes { get; set; } = 5;
    /// <summary>Доля от суммы заказа, возвращаемая баллами (0.03 = 3%).</summary>
    public decimal AccrualRate { get; set; } = 0.03m;
    /// <summary>Сколько рублей скидки даёт 1 балл (1 к 1).</summary>
    public decimal RubPerPoint { get; set; } = 1m;
    /// <summary>Максимальная доля заказа, которую можно оплатить баллами (0.5 = 50%).</summary>
    public decimal MaxRedeemShare { get; set; } = 0.5m;
    public decimal MaxOrderAmount { get; set; } = 1_000_000m;
}
