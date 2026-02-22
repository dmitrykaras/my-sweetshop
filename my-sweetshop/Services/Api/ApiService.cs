using my_sweetshop.Dtos;
using my_sweetshop.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace my_sweetshop.Services.Api
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient;

        static ApiService()
        {
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5107/"
                : "http://localhost:5107/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<List<ProductDto>> GetProductsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ProductDto>>("products") ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении продуктов: {ex.Message}");
                return new List<ProductDto>();
            }
        }

        public async Task<UserModel> GetUserAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserModel>("profile/me") ?? new UserModel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении профиля: {ex.Message}");
                return new UserModel();
            }
        }

        public async Task UpdateProfileAsync(string firstName, string lastName)
        {
            var dto = new UpdateProfileDto { FirstName = firstName, LastName = lastName };
            try
            {
                await _httpClient.PatchAsJsonAsync("/profile", dto);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении профиля: {ex.Message}");
            }
        }

        // Запрос на смену почты
        public async Task RequestChangeEmailAsync(string newEmail)
        {
            var dto = new AuthRequestChangeEmailDto { NewEmail = newEmail };
            await _httpClient.PostAsJsonAsync("profile/request-change-email", dto);
        }

        // Подтверждение смены почты
        public async Task ConfirmChangeEmail(string newEmail, string code)
        {
            var dto = new AuthConfirmChangeEmailDto
            {
                NewEmail = newEmail,
                Code = code
            };
            await _httpClient.PostAsJsonAsync("profile/confirm-change-email", dto);
        }
    }
}