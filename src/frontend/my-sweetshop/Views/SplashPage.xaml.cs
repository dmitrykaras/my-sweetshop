using my_sweetshop.Views.Auth;

namespace my_sweetshop.Views;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(200); // даём время отобразиться индикатору

        var token = await SecureStorage.GetAsync("access_token");

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
            return;

        if (!string.IsNullOrWhiteSpace(token))
        {
            // пользователь авторизован
            window.Page = new AppShell();
        }
        else
        {
            // пользователь не авторизован
            window.Page = new NavigationPage(new AuthStartPage());
        }
    }
}