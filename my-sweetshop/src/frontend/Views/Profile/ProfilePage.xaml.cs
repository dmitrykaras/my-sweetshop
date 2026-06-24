using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.ViewModels.Profile;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
using my_sweetshop.Views.Contact;
using my_sweetshop.Views.Profile.Cashier;
using System;
using System.Linq;

namespace my_sweetshop.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private int _secretTapCount = 0;
    private readonly IProfileService _profileService;

    public ProfilePage(EditProfileRootViewModel vm, IProfileService profileService)
    {
        InitializeComponent();
        BindingContext = vm;
        _profileService = profileService;
    }

    // Делаем запрос к серверу только, если данные ещё не загужены
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is EditProfileRootViewModel vm && vm.Profile != null)
        {
            // Если данные для этой сессии еще НЕ были загружены — дергаем сервер
            if (!vm.Profile.IsLoaded)
            {
                await vm.Profile.LoadUserAsync();
            }
        }
    }

    // Метод выхода из аккаунта
    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        // Проверка на случай, если процесс выхода уже запущен
        if (Application.Current?.MainPage is not Shell && Application.Current?.MainPage is not NavigationPage)
            return;

        bool confirm = await DisplayAlertAsync("Выход", "Вы действительно хотите выйти из профиля?", "Выйти", "Отмена");
        var authSession = Handler.MauiContext.Services.GetService<my_sweetshop.Services.Api.AuthSession>();

        if (!confirm)
            return;

        try
        {
            // Очистка сессии
            if (authSession != null)
            {
                // Вызываем ваш метод, который обнуляет Token, Email и чистит SecureStorage
                await authSession.LogoutAsync();
            }

            // Сбрасываем данные в синглтон-ViewModel
            if (BindingContext is EditProfileRootViewModel vm)
            {
                vm.Profile?.ClearData();
            }

            // Очистка сессии
            if (authSession != null)
            {
                await authSession.LogoutAsync();
            }

            // Очищаем кэш профиля
            if (_profileService != null)
            {
                _profileService.ClearCache();
            }

            // Очистка данных
            // Удаляем токены и флаги авторизации
            SecureStorage.Default.Remove("auth_token");
            SecureStorage.Default.Remove("refresh_token");
            Preferences.Default.Remove("is_logged_in");

            // Смена MainPage в главном потоке
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current != null)
                {
                    // Создаем чистую страницу авторизации
                    var authPage = new my_sweetshop.Views.Auth.AuthStartPage();

                    // Установка новой главной страницы полностью выгружает старый Shell из памяти
                    Application.Current.MainPage = new NavigationPage(authPage);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось корректно выйти: {ex.Message}", "ОК");
        }
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    private async void OnHelpTapped(object sender, EventArgs e)
    {
        var shell = Shell.Current;
        var tabBar = shell.Items.FirstOrDefault();
        if (tabBar == null) return;

        var contactSection = tabBar.Items.FirstOrDefault(section => section.Title == "Связаться");

        if (contactSection != null)
        {
            shell.CurrentItem = tabBar;
            shell.CurrentItem.CurrentItem = contactSection;
        }
    }

    private async void OnSiteTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://mysweetshop.ru");
    }

    private async void OnCashierSecretTapped(object sender, EventArgs e)
    {
        _secretTapCount++;
        if (_secretTapCount >= 5)
        {
            _secretTapCount = 0;
            await Shell.Current.GoToAsync(nameof(CashierPage));
        }
    }
}