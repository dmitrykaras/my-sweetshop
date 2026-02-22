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

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrWhiteSpace(_session.Token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(_session.Token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token);

            return await _http.SendAsync(request);
        }
    }
}