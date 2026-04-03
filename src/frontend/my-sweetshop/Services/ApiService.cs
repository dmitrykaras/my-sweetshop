using System.Net.Http.Json; // GetFromJsonAsync
using my_sweetshop.Models;
using System.Diagnostics; // Debug.WriteLine

namespace my_sweetshop.Services
{
    public class ApiService
    {
        // Основной клиент для запросов
        private static readonly HttpClient _httpClient;

        static ApiService()
        {
            // Определение адреса сервера
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "http://10.0.2.2:5107/api/"
                : "http://localhost:5107/api/";

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(10) //если сервер не ответит за 10 сек, запрос прервется
            };
        }

        /// <summary>
        /// Метод для получения списка всех продуктов с сервера
        /// </summary>
        public async Task<List<ProductDto>> GetProductsAsync()
        {
            try
            {
                // Метод автоматически превратит JSON от сервера в список объектов ProductDto
                var response = await _httpClient.GetFromJsonAsync<List<ProductDto>>("products");

                // Если сервер вернул null, возвращаем пустой список, чтобы приложение не вылетело
                return response ?? new List<ProductDto>();
            }
            catch (Exception ex)
            {
                // Если нет интернета, сервер выключен или ошибка в JSON — мы увидим это в консоли "Вывод" (Output)
                Debug.WriteLine(@"-----------------------------------------");
                Debug.WriteLine(@"ОШИБКА ПРИ ЗАПРОСЕ К API:");
                Debug.WriteLine(@"Сообщение: {0}", ex.Message);
                Debug.WriteLine(@"-----------------------------------------");

                return new List<ProductDto>();
            }
        }
    }
}