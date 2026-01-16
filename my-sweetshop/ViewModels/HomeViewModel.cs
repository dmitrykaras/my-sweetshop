using System.Collections.ObjectModel;
using System.Windows.Input;

namespace my_sweetshop.ViewModels;

public class HomeViewModel : BaseViewModel
{
    public ICommand OpenStoresCommand { get; }
    public ICommand OrderDeliveryCommand { get; }
    public ICommand OpenVkCommand { get; }
    public ICommand OpenTgCommand { get; }

    public HomeViewModel()
    {
        OpenStoresCommand = new Command(() =>
        {
            // позже: навигация к карте
        });

        OrderDeliveryCommand = new Command(() =>
        {
            Launcher.OpenAsync("https://t.me/MoyaKonditerakaya");
        });

        OpenTgCommand = new Command(async () => await SafeOpenUrl("https://t.me/MoyaKonditerakaya"));
        OpenVkCommand = new Command(async () => await SafeOpenUrl("https://vk.com/id196324878"));
    }

    private async Task SafeOpenUrl(string url)
    {
        try
        {
            // 1. Пытаемся открыть через системный Launcher (откроет приложение, если есть)
            bool opened = await Launcher.Default.OpenAsync(url);

            // 2. Если Launcher не справился, принудительно открываем в браузере
            if (!opened)
            {
                await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при открытии ссылки: {ex.Message}");
            // Здесь можно добавить DisplayAlert, если нужно оповестить пользователя
        }
    }
}