using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using System.Security.Claims;

namespace my_sweetshop.Views.Catalog
{
    [QueryProperty(nameof(Product), "Product")]
    public partial class ProductPage : ContentPage
    {
        private Product _product;
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                BindingContext = value;
                UpdateFavoriteIcon(); // Обновляем иконку при загрузке товара
            }
        }

        private readonly GetFavoriteProducts _getFavoriteProducts;
        public ProductPage(GetFavoriteProducts getFavoriteProducts)
        {
            InitializeComponent();
            _getFavoriteProducts = getFavoriteProducts;
        }

        private void UpdateFavoriteIcon()
        {
            if (Product != null)
            {
                FavoriteBtn.Source = Product.IsFavorite ? "favorite_image1.png" : "favorite_image2.png";
            }
        }

private async void OnFavoriteClicked(object sender, EventArgs e)
{
    // Проверяем, что товар вообще есть
    if (Product == null) return;

    try
    {
        // Получаем токен из SecureStorage
        string? token = await SecureStorage.Default.GetAsync("auth_token");

        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Вы не авторизованы. Пожалуйста, войдите в систему.", "ОК");
            return;
        }

        // Визуально переключаем сердечко сразу, чтобы пользователь не ждал ответа сервера (Optimistic UI)
        bool previousState = Product.IsFavorite;
        Product.IsFavorite = !previousState;

        UpdateFavoriteIcon();

        // Вызываем сервис для отправки запроса на бэкенд
        bool serverResult = await _getFavoriteProducts.ToggleFavoriteAsync(Product.Id, token);

        // Если бэкенд вернул что-то не то, откатываем изменения обратно
        if (serverResult != Product.IsFavorite)
        {
            Product.IsFavorite = serverResult;
            UpdateFavoriteIcon();
            await Shell.Current.DisplayAlertAsync("Ошибка", "Не удалось обновить статус избранного.", "ОК");
        }
    }
    catch (Exception)
    {
        // Обрабатка ошибок
        await Shell.Current.DisplayAlertAsync("Ошибка", "Проблемы с подключением к серверу.", "ОК");
        // Если была ошибка, возвращаем состояние назад
        Product?.IsFavorite = !Product.IsFavorite;
    }
}
    }
}