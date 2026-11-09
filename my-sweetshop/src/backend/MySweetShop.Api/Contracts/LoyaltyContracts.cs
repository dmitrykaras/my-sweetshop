namespace MySweetshop.Api.Contracts;

public record QrTokenResponse(string Token, DateTimeOffset ExpiresAt);

public record ScanRequest(string Token);
public record ScanResponse(
    Guid SessionId, // ID сессии
    string CustomerName, // Имя клиента
    int Points, // Бонусы
    decimal RubPerPoint,
    decimal MaxRedeemShare, // Максимальная доля для списания
    DateTimeOffset SessionExpiresAt);

// Запрос и ответ на начисление
public record AccrueRequest(decimal OrderAmount);
public record AccrueResponse(int Earned, int NewBalance);

// Запрос и ответ на списание
public record RedeemRequest(decimal OrderAmount, int PointsToRedeem);
public record RedeemResponse(decimal FinalAmount, int Redeemed, int NewBalance);