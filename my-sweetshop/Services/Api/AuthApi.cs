using System.Text.Json;
using my_sweetshop.Dtos;

namespace my_sweetshop.Services.Api
{
    public class AuthApi
    {
        private readonly ApiClient _api;

        public AuthApi(ApiClient api) => _api = api;

        public async Task RequestCodeAsync(string email)
        {
            var resp = await _api.PostAsync("auth/request-code", new { email });
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception(text);
        }

        public async Task<AuthVerifyCodeResponse> VerifyCodeAsync(string email, string code)
        {
            var resp = await _api.PostAsync("auth/verify-code", new { email, code });
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                ApiErrorResponse? apiError = null;

                try
                {
                    apiError = JsonSerializer.Deserialize<ApiErrorResponse>(text,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    // ignore parse errors
                }

                throw new ApiException((int)resp.StatusCode, apiError?.Message ?? text, apiError);
            }

            return JsonSerializer.Deserialize<AuthVerifyCodeResponse>(text,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}