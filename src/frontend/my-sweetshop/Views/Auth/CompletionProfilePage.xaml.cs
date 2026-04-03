using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using my_sweetshop.Models;
using System.Diagnostics;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace my_sweetshop.Views.Auth;

public partial class CompletionProfilePage : ContentPage
{
    private readonly HttpClient _httpClient;
    private readonly string _email;

    public CompletionProfilePage(string email)
    {
        InitializeComponent();

        string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5107/"
            : "http://localhost:5107/";

        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _email = email;

        _ = InitAsync(); // fire-and-forget
    }
    private async Task InitAsync()
    {
        try
        {
            var token = await SecureStorage.GetAsync("access_token"); // await вместо .Result

            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки токена: {ex}");
        }
    }

    // Крестик появляется, если поле заполнено > 1
    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        ClearFirstNameBtn.IsVisible = !string.IsNullOrWhiteSpace(FirstNameEntry.Text) && FirstNameEntry.Text.Length > 1;
        ClearLastNameBtn.IsVisible = !string.IsNullOrWhiteSpace(LastNameEntry.Text) && LastNameEntry.Text.Length > 1;

        ContinueBtn.IsEnabled =
            !string.IsNullOrWhiteSpace(FirstNameEntry.Text) && FirstNameEntry.Text.Length > 1 &&
            !string.IsNullOrWhiteSpace(LastNameEntry.Text) && LastNameEntry.Text.Length > 1;
    }

    // Удалить FirstName из поля
    private void OnClearFirstName(object sender, EventArgs e)
    {
        FirstNameEntry.Text = string.Empty;
    }

    // Удалить LastName из поля
    private void OnClearLastName(object sender, EventArgs e)
    {
        LastNameEntry.Text = string.Empty;
    }

    // Нажатие на кнопку продолжить
    private async void OnContinueClicked(object sender, EventArgs e)
    {
        ContinueBtn.IsEnabled = false;

        var request = new UserProfileRequest
        {
            Email = _email,
            FirstName = FirstNameEntry.Text.Trim(),
            LastName = LastNameEntry.Text.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Относительный путь, BaseAddress уже указан
            var response = await _httpClient.PatchAsync("profile/UpdateProfile", content);

            if (response.IsSuccessStatusCode)
            {
                await ShowToast("Профиль сохранён");

                var shell = MauiProgram.ServiceProvider.GetService<AppShell>();
                if (Application.Current?.Windows.Count > 0)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Ошибка", $"Не удалось сохранить профиль: {error}", "Ок");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок");
        }
        finally
        {
            ContinueBtn.IsEnabled = true;
        }
    }

    async Task ShowToast(string text)
    {
        var toast = Toast.Make(text, ToastDuration.Short);
        await toast.Show();
    }
}