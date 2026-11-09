namespace my_sweetshop.Dtos;

public record QrTokenResponse(string Token, DateTimeOffset ExpiresAt);

public record ScanResponse(
    Guid SessionId,
    string CustomerName,
    int Points,
    decimal RubPerPoint,
    decimal MaxRedeemShare,
    DateTimeOffset SessionExpiresAt);

public record AccrueResponse(int Earned, int NewBalance);

public record RedeemResponse(decimal FinalAmount, int Redeemed, int NewBalance);