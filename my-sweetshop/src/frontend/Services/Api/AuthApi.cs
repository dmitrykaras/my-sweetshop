using my_sweetshop.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace my_sweetshop.Services.Api
{
    public class AuthApi
    {
        private readonly HttpClient _http;

        public AuthApi(HttpClient http)
        {
            _http = http;
        }

        // Запрос кода подтвеждения
        public async Task RequestCodeAsync(string email)
        {
            var resp = await _http.PostAsJsonAsync("auth/request-code", new { Email = email });
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception(text);
        }

        // Попытка верификации кода
        public async Task<AuthVerifyCodeResponse> VerifyCodeAsync(string email, string code)
        {
            var resp = await _http.PostAsJsonAsync("auth/verify-code", new { email, code });
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