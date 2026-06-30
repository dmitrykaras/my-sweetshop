using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using System.Collections.ObjectModel;

namespace my_sweetshop.Views.Profile.FavoritesProduct
{
    public partial class FavoritesPage : ContentPage
    {
        // Сервис для работы с API
        private readonly GetFavoriteProducts _favoriteService;

        public ObservableCollection<Product> FavoriteProducts { get; set; } = new();

        // Передаем сервис через конструктор (DI)
        public FavoritesPage(GetFavoriteProducts favoriteService)
        {
            InitializeComponent();
            _favoriteService = favoriteService;
            FavoritesCollection.ItemsSource = FavoriteProducts;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadFavoritesAsync(); // Каждый раз при открытии страницы обновляем список
        }

        private async Task LoadFavoritesAsync()
        {
            try
            {
                // Запрашиваем токен
                string? token = await SecureStorage.Default.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await Shell.Current.DisplayAlertAsync("Ошибка", "Вы не авторизованы. Пожалуйста, войдите в систему.", "ОК");
                    return;
                }

                // Запрашиваем данные у сервера через сервис
                var products = await _favoriteService.GetFavoritesAsync(token);

                // Обновляем коллекцию на UI
                FavoriteProducts.Clear();
                foreach (var item in products)
                {
                    FavoriteProducts.Add(item);
                }
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось загрузить список избранного. Проверьте подключение.", "ОК");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnProductTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is Product tappedProduct)
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "Product", tappedProduct }
                };

                await Shell.Current.GoToAsync(nameof(Catalog.ProductPage), navigationParameter);
            }
        }
    }
}