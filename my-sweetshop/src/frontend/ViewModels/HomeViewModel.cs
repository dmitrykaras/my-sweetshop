using System.Collections.ObjectModel;
using System.Windows.Input;

namespace my_sweetshop.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public ICommand OpenStoresCommand { get; }
        public ICommand OrderDeliveryCommand { get; }
        public ICommand OpenVkCommand { get; }
        public ICommand OpenTgCommand { get; }
        public ICommand OpenCatalogCommand { get; }
        public ICommand OpenPointsCommand { get; }

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

            OpenCatalogCommand = new Command(NavigateToCatalog);
            OpenPointsCommand = new Command(NavigateToPoints);
        }

        // Безопасное открытие ссылки
        private async Task SafeOpenUrl(string url)
        {
            try
            {
                // Пытаемся открыть через системный Launcher (откроет приложение, если есть)
                bool opened = await Launcher.Default.OpenAsync(url);

                // Если Launcher не справился, принудительно открываем в браузере
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

        // Логика перехода на вкладку Каталог
        private void NavigateToCatalog()
        {
            var shell = Shell.Current;
            if (shell == null) return;

            var tabBar = shell.Items.FirstOrDefault();
            if (tabBar == null) return;

            var catalogSection = tabBar.Items.FirstOrDefault(section => section.Title == "Каталог");
            if (catalogSection != null)
            {
                shell.CurrentItem = tabBar;

                shell.CurrentItem?.CurrentItem = catalogSection;
            }
        }

        // Логика перехода на вкладку Профиль (Баллы)
        private void NavigateToPoints()
        {
            var shell = Shell.Current;
            if (shell == null) return;

            var tabBar = shell.Items.FirstOrDefault();
            if (tabBar == null) return;

            var contactSection = tabBar.Items.FirstOrDefault(section => section.Title == "Профиль");
            if (contactSection != null)
            {
                shell.CurrentItem = tabBar;
                
                shell.CurrentItem?.CurrentItem = contactSection;
            }
        }
    }
}