using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.ViewModels.Profile;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
using my_sweetshop.Views.Contact;
using my_sweetshop.Views.Profile.Cashier;
using my_sweetshop.Views.Profile.FavoritesProduct;
using System;
using System.Linq;

namespace my_sweetshop.Views.Profile;

public partial class ProfilePage : ContentPage
{
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
            // Если данные для этой сессии еще не были загружены — запрашиваем у сервера
            if (!vm.Profile.IsLoaded)
            {
                await vm.Profile.LoadUserAsync();
            }
        }
    }

    // Метод выхода из аккаунта
    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        // Получаем текущую страницу активного окна для проверки
        var currentPage = this.Window?.Page;

        // Проверка на случай, если процесс выхода уже запущен
        if (currentPage is not Shell && currentPage is not NavigationPage)
            return;

        bool confirm = await DisplayAlertAsync("Выход", "Вы действительно хотите выйти из профиля?", "Выйти", "Отмена");

        var authSession = Handler?.MauiContext?.Services.GetService<my_sweetshop.Services.Api.AuthSession>();

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

            // Смена страницы в главном потоке
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Используем Window текущей страницы
                if (this.Window != null)
                {
                    var authPage = new my_sweetshop.Views.Auth.AuthStartPage();

                    // Меняем корневую страницу окна. Это выгрузит старый Shell из памяти
                    this.Window.Page = new NavigationPage(authPage);
                }
                // Резервный вариант, если Window почему-то не привязан
                else if (Application.Current?.Windows.Count > 0)
                {
                    var authPage = new my_sweetshop.Views.Auth.AuthStartPage();
                    Application.Current.Windows[0].Page = new NavigationPage(authPage);
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось корректно выйти: {ex.Message}", "ОК");
        }
    }

    // Нажатие на профиль чтобы его изменить
    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    // Нажатие на кноку поддержки
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

    // Нажатие на кнопку сайта
    private async void OnSiteTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://mysweetshop.ru");
    }

    // Нажатие на кнопку избранного
    private async void OnFavoritesTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(FavoritesPage));
    }
}