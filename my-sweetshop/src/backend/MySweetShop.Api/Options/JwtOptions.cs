namespace MySweetShop.Api.Options;

public class JwtOptions
{
    // Издатель токена
    public string Issuer { get; set; } = default!;
    // Получатель токена
    public string Audience { get; set; } = default!;
    // Ключ подписания токена
    public string Key { get; set; } = default!;
    // Время жизни токена
    public int ExpiresMinutes { get; set; } = 30;
}