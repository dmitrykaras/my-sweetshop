using my_sweetshop.Dtos; //ChangeEmailRequest
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
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

        // Метод для отправки code: при успехе отправляет код на почту
        public async Task RequestCodeAsync(string Email, string NewEmail)
        {
            var payload = new { Email, NewEmail };

            var resp = await _http.PostAsJsonAsync("profile/request-code-for-change-email", payload);
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception(text);
        }

        // Метод для проверки code: при успехе меняет почту Email на NewEmail
        public async Task<bool> VerifyCodeForChangeEmail(string newEmail, string code, string token)
        {
            var req = new { NewEmail = newEmail, Code = code };
            var httpReq = new HttpRequestMessage(HttpMethod.Post, "profile/verify-change-email")
            {
                Content = JsonContent.Create(req)
            };

            // Добавляем JWT
            httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(httpReq);

            if (resp.IsSuccessStatusCode)
            {
                // 3. Вызываем загрузку у синглтона
                var vm = MauiProgram.ServiceProvider.GetRequiredService<UserProfileViewModel>();
                await vm.LoadUserAsync();

                return true;
            }

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

            // 2. Обновляем сессию
            //var session = MauiProgram.ServiceProvider.GetRequiredService<AuthSession>();
            //await session.SetSessionAsync(session.Token, newEmail);

            throw new Exception(errorMessage);
        }
    }
}