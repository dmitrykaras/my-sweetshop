namespace my_sweetshop.Services.AuthStep
{
    public class AuthSession
    {
        private const string TokenKey = "auth_token";

        public string? Token { get; private set; }

        public async Task InitializeAsync()
        {
            Token = await SecureStorage.GetAsync(TokenKey);
        }

        public async Task SetTokenAsync(string token)
        {
            Token = token;
            await SecureStorage.SetAsync(TokenKey, token);
        }

        public async Task LogoutAsync()
        {
            Token = null;
            SecureStorage.Remove(TokenKey);
        }

        public bool IsAuthorized => !string.IsNullOrWhiteSpace(Token);
    }
}
