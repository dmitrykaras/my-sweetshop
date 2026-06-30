using my_sweetshop.Services.Api;
using my_sweetshop.Services.UserService;
using my_sweetshop.Views;
using my_sweetshop.Views.Auth;

namespace my_sweetshop;

public partial class App : Application
{
    private readonly IUserService _userService;
    private readonly AuthSession _authSession;

    public App(IUserService userService, AuthSession authSession)
    {
        InitializeComponent();
        _userService = userService;
        _authSession = authSession;

        // Запускаем единый процесс инициализации
        StartWork();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Создаем окно и сразу задаем ему корневую страницу
        return new Window(new SplashPage());
    }

    private async void StartWork()
    {
        try
        {
            // Небольшая задержка для отображения индикатора на SplashPage
            await Task.Delay(500);

            // Восстанавливаем сессию (токен) из SecureStorage
            await _authSession.InitializeAsync();

            // Если токен есть, проверяем его валидность запросом к профилю
            if (_authSession.IsAuthorized)
            {
                try
                {
                    await _userService.GetCurrentUser();
                }
                catch (Exception ex)
                {
                    // Если токен протух или сервер вернул 401
                    System.Diagnostics.Debug.WriteLine($"Auth failed: {ex.Message}");
                    await _authSession.LogoutAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Init error: {ex.Message}");
        }
        finally
        {
            // Переключаем интерфейс
            UpdateMainPage();
        }
    }

    // Универсальный метод переключения между Shell и Авторизацией
    public void UpdateMainPage()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var currentWindow = Application.Current?.Windows.Count > 0
            ? Application.Current.Windows[0]
            : null;

            if (currentWindow != null)
            {
                if (_authSession.IsAuthorized)
                {
                    // Основное приложение
                    currentWindow.Page = new AppShell();
                }
                else
                {
                    // Стек авторизации
                    currentWindow.Page = new NavigationPage(new AuthStartPage());
                }
            }
        });
    }
}