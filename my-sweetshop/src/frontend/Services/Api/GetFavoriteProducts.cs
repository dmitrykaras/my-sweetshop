using my_sweetshop.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace my_sweetshop.Services.Api;

public class GetFavoriteProducts
{
    private readonly HttpClient _httpClient;

    // Создаем ленивые настройки, которые разрешают читать числа из строк
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString |
                         JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public GetFavoriteProducts(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Метод получения избранных товаров
    public async Task<List<Product>> GetFavoritesAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "products/favorites");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Передаем настройки _jsonOptions вторым аргументом
                return await response.Content.ReadFromJsonAsync<List<Product>>(_jsonOptions) ?? new();
            }

            return new List<Product>();
        }
        catch (Exception ex)
        {
            return new List<Product>();
        }
    }

    // Метод добавление/удаления избранных товаров
    public async Task<bool> ToggleFavoriteAsync(Guid productId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"products/{productId}/toggle-favorite");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                // Читаем ответ от сервера как чистый текст
                string rawJson = await response.Content.ReadAsStringAsync();

                // Проверяем, что пришло
                if (bool.TryParse(rawJson, out bool isFavoriteResult))
                {
                    return isFavoriteResult;
                }

                // Если бэк вернул JSON-объект, парсим его с нашими настройками _jsonOptions
                var result = JsonSerializer.Deserialize<FavoriteResponse>(rawJson, _jsonOptions);
                return result?.IsFavorite ?? false;
            }
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public class FavoriteResponse { public bool IsFavorite { get; set; } }
}