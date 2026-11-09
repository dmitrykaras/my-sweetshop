using my_sweetshop.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace my_sweetshop.Services.Api;

public class LoyaltyApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class LoyaltyApi(HttpClient http)
{
    // Клиент: получить токен для QR
    public Task<QrTokenResponse> GenerateQrAsync(CancellationToken ct = default) =>
        PostAsync<QrTokenResponse>("api/qr/generate", null, ct);

    // Кассир
    public Task<ScanResponse> ScanAsync(string token, CancellationToken ct = default) =>
        PostAsync<ScanResponse>("api/cashier/scan", new { token }, ct);

    public Task<AccrueResponse> AccrueAsync(Guid sessionId, decimal orderAmount, CancellationToken ct = default) =>
        PostAsync<AccrueResponse>($"api/cashier/sessions/{sessionId}/accrue", new { orderAmount }, ct);

    public Task<RedeemResponse> RedeemAsync(Guid sessionId, decimal orderAmount, int pointsToRedeem, CancellationToken ct = default) =>
        PostAsync<RedeemResponse>($"api/cashier/sessions/{sessionId}/redeem", new { orderAmount, pointsToRedeem }, ct);

    private async Task<T> PostAsync<T>(string url, object? body, CancellationToken ct)
    {
        using var resp = await http.PostAsync(url, JsonContent.Create(body ?? new { }), ct);

        if (!resp.IsSuccessStatusCode)
            throw new LoyaltyApiException(await ReadMessageAsync(resp, ct), (int)resp.StatusCode);

        return (await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct))!;
    }

    private static async Task<string> ReadMessageAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var err = await resp.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: ct);
            if (!string.IsNullOrWhiteSpace(err?.Message)) return err.Message;
        }
        catch { /* тело не JSON — используем текст по умолчанию */ }

        return resp.StatusCode == HttpStatusCode.Forbidden ? "Нет прав кассира" : "Ошибка сервера";
    }
}