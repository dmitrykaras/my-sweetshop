using my_sweetshop.Dtos;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using my_sweetshop.Services.Api;

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
        // Клонируем запрос ДО первой отправки. 
        // HttpClient уничтожает тело запроса (Content) после SendAsync, клонировать потом - нельзя.
        var clonedRequestForRetry = await CloneRequestAsync(request);

        try
        {
            // Прикрепляем текущий токен
            if (!string.IsNullOrEmpty(_session.Token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);

            var response = await base.SendAsync(request, ct);

            // Проверяем, не является ли сам этот запрос запросом к авторизации
            bool isAuthEndpoint = request.RequestUri?.ToString().Contains("auth", StringComparison.OrdinalIgnoreCase) == true;

            // Если 401 и это обычный защищенный метод
            if (response.StatusCode == HttpStatusCode.Unauthorized && !isAuthEndpoint)
            {
                await _refreshSemaphore.WaitAsync(ct);
                try
                {
                    // Проверяем, не обновил ли токен другой параллельный запрос (пока мы ждали очередь)
                    if (request.Headers.Authorization?.Parameter != _session.Token && !string.IsNullOrEmpty(_session.Token))
                    {
                        clonedRequestForRetry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.Token);
                        return await base.SendAsync(clonedRequestForRetry, ct);
                    }

                    var refreshToken = await SecureStorage.Default.GetAsync("refresh_token");
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        System.Diagnostics.Debug.WriteLine("[JwtAuthHandler] ВНИМАНИЕ: RefreshToken отсутствует в SecureStorage. Прерывание рефреша.");
                        return response;
                    }

                    System.Diagnostics.Debug.WriteLine("[JwtAuthHandler] Отправка запроса на auth/refresh...");

                    // Запрашиваем новый токен
                    var refreshResponse = await _refreshClient.PostAsJsonAsync("auth/refresh", new { RefreshToken = refreshToken }, ct);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var tokenData = await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
                        if (tokenData != null && !string.IsNullOrEmpty(tokenData.Token))
                        {
                            System.Diagnostics.Debug.WriteLine("[JwtAuthHandler] УСПЕХ: Токен обновлен!");

                            await _session.SetTokenAsync(tokenData.Token);
                            await SecureStorage.Default.SetAsync("refresh_token", tokenData.RefreshToken);

                            // Повторяем изначальный запрос с новым токеном
                            clonedRequestForRetry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.Token);
                            return await base.SendAsync(clonedRequestForRetry, ct);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[JwtAuthHandler] ОШИБКА: Сервер вернул {refreshResponse.StatusCode} при попытке рефреша.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[JwtAuthHandler Ошибка внутри рефреша]: {ex.Message}");
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }
            return response;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[JwtAuthHandler Critical Error]: {ex.Message}");
            throw;
        }
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        foreach (var h in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }

        if (request.Content != null)
        {
            // Обязательно вычитываем байты до того, как оригинал будет отправлен
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);
            foreach (var h in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
        }
        return clone;
    }
}