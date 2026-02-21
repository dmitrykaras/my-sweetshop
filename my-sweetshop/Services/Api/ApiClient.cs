using my_sweetshop.Services.AuthStep;
using System.Net.Http.Headers;
using System.Text;
using my_sweetshop.Dtos;
using System.Net.Http.Json;

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



        public async Task<HttpResponseMessage> PostAsync(string path, object body, bool auth = false)
        {
            if (auth && _session.IsAuthorized)
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _session.Token);

            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _http.PostAsync(path, content);
        }

        public async Task<HttpResponseMessage> GetAsync(string path, bool auth = false)
        {
            if (auth && _session.IsAuthorized)
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _session.Token);

            return await _http.GetAsync(path);
        }
    }
}
