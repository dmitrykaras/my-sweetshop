using my_sweetshop.Views.Auth;
using my_sweetshop.Views;

namespace my_sweetshop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = new NavigationPage(MauiProgram.ServiceProvider.GetService<AuthStartPage>()!);

        return new Window(navPage);
        //return new Window(new SplashPage()); //поменять после тестирования регистрации/авторизации
    }
}