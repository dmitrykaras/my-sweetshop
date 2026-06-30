using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace my_sweetshop.Views.Auth;

public partial class CompletionProfilePage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly string _email;
    private readonly IServiceProvider _serviceProvider;

    public CompletionProfilePage(string email, AuthApi authApi, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authApi = authApi;
        _email = email;
        _serviceProvider = serviceProvider;

        ContinueBtn.IsEnabled = false;
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
        if (!ContinueBtn.IsEnabled) return;

        ContinueBtn.IsEnabled = false;
        try
        {
            var firstName = FirstNameEntry.Text.Trim();
            var lastName = LastNameEntry.Text.Trim();

            // Относительный путь, BaseAddress уже указан
            await _authApi.UpdateProfileAsync(firstName, lastName);

            await ShowToast("Профиль сохранён");

            // Переход в основное приложение
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current != null)
                {
                    // Запрашиваем AppShell из DI со всеми свежими зависимостями и токенами
                    var freshAppShell = _serviceProvider.GetRequiredService<AppShell>();
                    Application.Current.Windows[0].Page = freshAppShell;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MAUI_LOG] Ошибка сохранения профиля: {ex.Message}");
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок");
            ContinueBtn.IsEnabled = true;
        }
    }

    // Отображение всплывающих уведомлений
    async Task ShowToast(string text)
    {
        var toast = Toast.Make(text, ToastDuration.Short);
        await toast.Show();
    }
}