using my_sweetshop.Views.Auth;
using my_sweetshop.Views;
using my_sweetshop.Services.UserService;

namespace my_sweetshop;

public partial class App : Application
{
    private readonly IUserService _userService;
    public App(IUserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navPage = new NavigationPage(MauiProgram.ServiceProvider.GetService<AuthStartPage>()!);

        //return new Window(navPage);
        return new Window(new SplashPage()); //поменять после тестирования регистрации/авторизации
    }

    protected override async void OnStart()
    {
        var session = MauiProgram.ServiceProvider.GetRequiredService<AuthSession>();
        await session.InitializeAsync();

        await InitUserAsync();
    }

    // Метод загрузки данных пользователя при запуске приложения
    private async Task InitUserAsync()
    {
        try
        {
            var profile = await _userService.GetCurrentUser();
        }
        catch (Exception ex)
        {
            // Обработка ошибок по типу нет интернета и тд
        }
    }
}