using System.Text;


namespace my_sweetshop.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly AuthSession _session;

        public ApiClient(AuthSession session)
        {
            _session = session;

            _http = new HttpClient
            {
                BaseAddress = new Uri("http://10.0.2.2:5107/api/")
            };
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object body, bool auth = false)
        {
            if (auth && _session.IsAuthorized)
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token);

            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _http.PostAsync(path, content);
        }

        public async Task<HttpResponseMessage> GetAsync(string path, bool auth = false)
        {
            if (auth && _session.IsAuthorized)
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.Token);

            return await _http.GetAsync(path);
        }
    }
}
