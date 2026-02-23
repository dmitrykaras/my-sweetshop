using my_sweetshop.Dtos; //ChangeEmailRequest
using System.Net.Http.Json;


namespace my_sweetshop.Services.Api
{
    public class ChangeEmail
    {
        private readonly HttpClient _http;

        public ChangeEmail(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> ChangeEmailAsync(string newEmail, string code, string token)
        {
            var req = new { NewEmail = newEmail, Code = code };
            var httpReq = new HttpRequestMessage(HttpMethod.Post, "profile/confirm-change-email")
            {
                Content = JsonContent.Create(req)
            };

            // Добавляем JWT
            httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // В ChangeEmail.cs
            var resp = await _http.SendAsync(httpReq);

            if (resp.IsSuccessStatusCode) return true;

            string errorMessage;
            try
            {
                // Пробуем прочитать как JSON
                var err = await resp.Content.ReadFromJsonAsync<ApiErrorResponse>();
                errorMessage = err?.Message ?? "Неизвестная ошибка";
            }
            catch
            {
                // Если не JSON — читаем как обычную строку
                errorMessage = await resp.Content.ReadAsStringAsync();
            }

            throw new Exception(errorMessage);
        }

        // Верификация кода
        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var req = new AuthVerifyCodeRequest
            {
                Email = email,
                Code = code
            };

            var resp = await _http.PostAsJsonAsync("profile/verify-code", req);

            if (resp.IsSuccessStatusCode)
                return true;

            var err = await resp.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new Exception(err?.Message ?? "Ошибка проверки кода");
        }
    }
}
