using my_sweetshop;
using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Services.UserService;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
using System.Net.Http.Json;

public class ProfileService : IProfileService
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private UserModel? _cachedUser;

    public ProfileService(HttpClient httpClient) => _httpClient = httpClient;

    // Метод для получения данных пользователя
    // если forceRefresh = false и кэш пользователя не пустой, то возвращаем кэш
    public async Task<UserModel?> GetProfileAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedUser != null) return _cachedUser;

        await _semaphore.WaitAsync();
        try
        {
            // Двойная проверка после входа в семафор
            if (!forceRefresh && _cachedUser != null) return _cachedUser;

            var response = await _httpClient.GetAsync("profile/me");
            if (response.IsSuccessStatusCode)
            {
                _cachedUser = await response.Content.ReadFromJsonAsync<UserModel>();
            }
            return _cachedUser;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, "profile")
        {
            Content = JsonContent.Create(dto)
        };

        var response = await _httpClient.SendAsync(req);

        if (response.IsSuccessStatusCode)
        {
            // Обновляем кэш сразу, чтобы UI мгновенно "ожил"
            var updated = await response.Content.ReadFromJsonAsync<UserModel>();
            _cachedUser = updated;
            return true;
        }
        return false;
    }

    // Запрос на смену почты

    public async Task RequestChangeEmailAsync(string newEmail)

    {

        var dto = new AuthRequestChangeEmailDto { NewEmail = newEmail };

        await _httpClient.PostAsJsonAsync("profile/request-change-email", dto);

    }

    // Подтверждение смены почты
    //public async Task ConfirmChangeEmail(string newEmail, string code)
    //{
    //    var dto = new AuthConfirmChangeEmailDto { NewEmail = newEmail, Code = code };
    //    await _httpClient.PostAsJsonAsync("profile/confirm-change-email", dto);

    //    // 1. Обязательно сбрасываем кэш сервиса!
    //    _cachedUser = null;

    //    // 2. Обновляем сессию
    //    var session = MauiProgram.ServiceProvider.GetRequiredService<AuthSession>();
    //    await session.SetSessionAsync(session.Token, newEmail);

    //    // 3. Вызываем загрузку у синглтона
    //    var vm = MauiProgram.ServiceProvider.GetRequiredService<UserProfileViewModel>();
    //    await vm.LoadUserAsync();
    //}

    public void ClearCache() => _cachedUser = null;
}