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

        // 1. Устанавливаем заставку как стартовую страницу
        MainPage = new SplashPage();

        // 2. Запускаем единый процесс инициализации
        StartWork();
    }

    private async void StartWork()
    {
        try
        {
            // Небольшая задержка для отображения индикатора на SplashPage
            await Task.Delay(500);

            // 3. Восстанавливаем сессию (токен) из SecureStorage
            await _authSession.InitializeAsync();

            // 4. Если токен есть, проверяем его валидность запросом к профилю
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
            // 5. Переключаем интерфейс
            UpdateMainPage();
        }
    }

    // Универсальный метод переключения между Shell и Авторизацией.
    public void UpdateMainPage()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_authSession.IsAuthorized)
            {
                // Основное приложение
                MainPage = new AppShell();
            }
            else
            {
                // Стек авторизации
                MainPage = new NavigationPage(new AuthStartPage());
            }
        });
    }

    protected override void OnStart()
    {
        // Вся логика теперь управляется через StartWork
    }
}