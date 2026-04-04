namespace my_sweetshop.Services.Api;

public class AuthSession
{
    private const string TokenKey = "auth_token";
    private const string EmailKey = "auth_email";

    public string? Token { get; private set; }
    public string? Email { get; set; }

    // Получение токена и почты
    public async Task InitializeAsync()
    {
        Token = await SecureStorage.GetAsync(TokenKey);
        Email = await SecureStorage.GetAsync(EmailKey);
    }

    // Установка токена и почты
    public async Task SetSessionAsync(string token, string email)
    {
        Token = token;
        Email = email;
        await SecureStorage.SetAsync(TokenKey, token);
        await SecureStorage.SetAsync(EmailKey, email);
    }

    // Выход из системы
    public async Task LogoutAsync()
    {
        Token = null;
        Email = null;
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(EmailKey);

        Preferences.Default.Remove("is_logged_in");
    }

    // Сохраняем токен в памяти и в SecureStorage
    public async Task SetTokenAsync(string token) { Token = token; await SecureStorage.SetAsync(TokenKey, token); }

    // Проверка токена
    public bool IsAuthorized => !string.IsNullOrWhiteSpace(Token);
}