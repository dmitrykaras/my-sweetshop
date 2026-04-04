using my_sweetshop.Dtos;
using my_sweetshop.Models;
using System.Diagnostics;
using System.Net.Http.Json;

namespace my_sweetshop.Services.Api
{
    public class GetProducts
    {
        private readonly HttpClient _httpClient;

        public GetProducts(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
    }
}