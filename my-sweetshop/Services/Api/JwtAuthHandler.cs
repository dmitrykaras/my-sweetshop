using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using my_sweetshop.Dtos;

public class JwtAuthHandler : DelegatingHandler
{
    private readonly AuthSession _session;
    private readonly HttpClient _refreshClient;

    public JwtAuthHandler(AuthSession session, HttpClient refreshClient)
    {
        _session = session;
        _refreshClient = refreshClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // 1. Подставляем токен из сессии (всегда актуальный)
        if (!string.IsNullOrEmpty(_session.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);

        var response = await base.SendAsync(request, cancellationToken);

        // 2. Если 401 — пытаемся обновиться
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshToken = await SecureStorage.GetAsync("refresh_token");
            if (string.IsNullOrEmpty(refreshToken)) return response;

            // Используем отдельный клиент для рефреша (как у вас и сделано)
            var refreshResp = await _refreshClient.PostAsJsonAsync(
                "auth/refresh",
                new { RefreshToken = refreshToken }
            );

            if (refreshResp.IsSuccessStatusCode)
            {
                var tokenResp = await refreshResp.Content.ReadFromJsonAsync<TokenResponse>();
                var newJwt = tokenResp!.Token;

                // 1. Обновляем JWT в памяти
                await _session.SetTokenAsync(newJwt);

                // 2. ВАЖНО: Если сервер прислал НОВЫЙ рефреш-токен, сохраняем его
                if (!string.IsNullOrEmpty(tokenResp.RefreshToken))
                {
                    await SecureStorage.SetAsync("refresh_token", tokenResp.RefreshToken);
                }

                // 3. Повторяем запрос
                var newRequest = await CloneRequest(request); // Добавили await
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newJwt);

                return await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    // Вспомогательный метод для клонирования (HttpRequestMessage нельзя использовать дважды)
    private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // Копируем заголовки запроса
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        // Клонируем контент (тело запроса), если оно есть
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            var streamContent = new StreamContent(ms);
            // Копируем заголовки контента (Content-Type и т.д.)
            foreach (var header in request.Content.Headers)
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Content = streamContent;
        }

        return clone;
    }
}