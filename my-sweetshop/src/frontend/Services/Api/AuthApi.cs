using my_sweetshop.Dtos;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace my_sweetshop.Services.Api
{
    public class AuthApi(HttpClient http)
    {
        private readonly HttpClient _http = http;

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

        // Метод для обработки и отправки firstname и lastname на последнем этапе регистрации
        public async Task UpdateProfileAsync(string firstName, string lastName)
        {
            try
            {
                // Получаем токен из защищенного хранилища
                var token = await SecureStorage.GetAsync("auth_token");
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("Авторизационный токен не найден. Пожалуйста, войдите снова.");
                }

                // Подготавливаем данные запроса
                var updateData = new
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Создаем запрос. 
                // Используем PATCH для частичного обновления профиля, как в логах Docker.
                using var request = new HttpRequestMessage(HttpMethod.Patch, "profile/UpdateProfile");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = content;

                Debug.WriteLine($"[MAUI_LOG] AuthApi: Отправка данных профиля на {_http.BaseAddress}profile/UpdateProfile");

                // Отправляем запрос
                var response = await _http.SendAsync(request);

                // Проверяем результат
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[MAUI_LOG] AuthApi Error: {response.StatusCode} - {errorContent}");
                    throw new Exception($"Не удалось сохранить данные профиля. Статус: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[MAUI_LOG] AuthApi Network Error: {ex.Message}");
                throw new Exception("Ошибка сети при обновлении профиля. Проверьте соединение.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MAUI_LOG] AuthApi Critical Error: {ex.Message}");
                throw;
            }
        }

    }
}