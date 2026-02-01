using my_sweetshop.ViewModels.Dtos;


namespace my_sweetshop.Services
{
    public class AuthApi
    {
        private readonly ApiClient _api;

        public AuthApi(ApiClient api) => _api = api;

        public async Task RequestCodeAsync(string email)
        {
            var resp = await _api.PostAsync("auth/request-code", new
            {
                email
            });

            if (!resp.IsSuccessStatusCode)
                throw new Exception(await resp.Content.ReadAsStringAsync());
        }

        public async Task<AuthVerifyCodeResponse> VerifyCodeAsync(string email, string code)
        {
            var resp = await _api.PostAsync("auth/verify-code", new
            {
                email,
                code
            });

            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception(text);

            return System.Text.Json.JsonSerializer.Deserialize<AuthVerifyCodeResponse>(text,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}
