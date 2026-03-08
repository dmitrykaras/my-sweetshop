using my_sweetshop.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class JwtAuthHandler : DelegatingHandler
{
    private readonly AuthSession _session;
    private readonly HttpClient _refreshClient;
    private readonly SemaphoreSlim _refreshSemaphore = new SemaphoreSlim(1, 1);

    public JwtAuthHandler(AuthSession session, HttpClient refreshClient)
    {
        _session = session;
        _refreshClient = refreshClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            // 1. Прикрепляем токен
            if (!string.IsNullOrEmpty(_session.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);

            var response = await base.SendAsync(request, ct);

            // 2. Если 401 и это НЕ запрос к самой авторизации
            if (response.StatusCode == HttpStatusCode.Unauthorized &&
                request.RequestUri?.AbsolutePath.Contains("auth") == false)
            {
                await _refreshSemaphore.WaitAsync(ct);
                try
                {
                    if (request.Headers.Authorization?.Parameter != _session.Token)
                    {
                        return await RetryRequest(request, _session.Token, ct);
                    }

                    // Используем Default.GetAsync для стабильности на Android
                    var refreshToken = await SecureStorage.Default.GetAsync("refresh_token");
                    if (string.IsNullOrEmpty(refreshToken)) return response;

                    var refreshResponse = await _refreshClient.PostAsJsonAsync("auth/refresh", new { RefreshToken = refreshToken });

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var tokenData = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>();
                        if (tokenData != null)
                        {
                            await _session.SetTokenAsync(tokenData.Token);
                            await SecureStorage.Default.SetAsync("refresh_token", tokenData.RefreshToken);

                            return await RetryRequest(request, tokenData.Token, ct);
                        }
                    }
                }
                finally { _refreshSemaphore.Release(); }
            }
            return response;
        }
        catch (Exception ex)
        {
            // Это поймает ошибку до того, как она убьет Java-прокси
            System.Diagnostics.Debug.WriteLine($"[JwtAuthHandler Error]: {ex}");
            throw;
        }
    }

    private async Task<HttpResponseMessage> RetryRequest(HttpRequestMessage request, string token, CancellationToken ct)
    {
        var clone = await CloneRequest(request);
        clone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(clone, ct);
    }

    private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
    {
        // МАКСИМАЛЬНО простое клонирование для Android
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        // Копируем только те заголовки, которые не вызывают конфликтов
        foreach (var h in request.Headers)
        {
            if (h.Key.ToLower() != "authorization")
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        // Если это GET запрос (как профиль), контент копировать НЕ НУЖНО
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);
            foreach (var h in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        return clone;
    }
}