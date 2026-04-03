using my_sweetshop.Dtos;
using my_sweetshop.Services.AuthStep;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace my_sweetshop.Services.Api
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly AuthSession _session;

        public ApiClient(HttpClient http, AuthSession session)
        {
            _http = http;
            _session = session;
        }

        public Task<HttpResponseMessage> GetAsync(string url)
            => _http.GetAsync(url);

        public Task<HttpResponseMessage> PostAsync(string url, object data)
            => _http.PostAsJsonAsync(url, data);
    }
}