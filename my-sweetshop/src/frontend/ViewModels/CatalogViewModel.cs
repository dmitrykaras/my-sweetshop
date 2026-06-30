using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Views.Catalog;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace my_sweetshop.ViewModels
{
    public class CatalogViewModel : BindableObject
    {
        private readonly GetProducts _apiService;

        public ObservableCollection<Product> Desserts { get; } = new();
        public ObservableCollection<Product> MaleCakes { get; } = new();
        public ObservableCollection<Product> FemaleCakes { get; } = new();
        public ObservableCollection<Product> KidsCakes { get; } = new();
        public ObservableCollection<Product> WeddingCakes { get; } = new();
        public ObservableCollection<Product> Sets { get; } = new();
        public ObservableCollection<Product> Drinks { get; } = new();

        // Свойство для контроля RefreshView
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                if (_isRefreshing != value)
                {
                    _isRefreshing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenProductCommand { get; }

        public ICommand RefreshCommand { get; }

        public CatalogViewModel(GetProducts apiService)
        {
            _apiService = apiService; // Теперь DI передает сюда настроенный сервис

            OpenProductCommand = new Command<Product>(OpenProduct);

            RefreshCommand = new Command(async () => await LoadDataAsync());

            // Сразу запускаем загрузку
            _ = LoadDataAsync();
        }

        // Загрузка данных продуктов
        private async Task LoadDataAsync()
        {
            var dtos = await _apiService.GetProductsAsync();

            // Все обновления коллекций — в главном потоке (UI Thread)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ClearAll();

                foreach (var dto in dtos)
                {
                    // Маппинг (превращаем Dto в Product)
                    var product = new Product
                    {
                        Id = dto.Id!,
                        Name = dto.Name!,
                        Description = dto.Description ?? "",
                        Price = dto.Price ?? 0,
                        ImageUrl = dto.ImageUrl ?? "placeholder.png",
                        CategoryId = dto.CategoryId.ToString()
                    };

                    // Распределяем по спискам на основе ID из твоего AppDbContext сервера
                    var catId = dto.CategoryId.ToString().ToLower();

                    switch (catId)
                    {
                        case "11111111-1111-1111-1111-111111111111": Desserts.Add(product); break;
                        case "22222222-2222-2222-2222-222222222222": MaleCakes.Add(product); break;
                        case "33333333-3333-3333-3333-333333333333": FemaleCakes.Add(product); break;
                        case "44444444-4444-4444-4444-444444444444": KidsCakes.Add(product); break;
                        case "55555555-5555-5555-5555-555555555555": WeddingCakes.Add(product); break;
                        case "66666666-6666-6666-6666-666666666666": Sets.Add(product); break;
                        case "77777777-7777-7777-7777-777777777777": Drinks.Add(product); break;
                    }
                }
                IsRefreshing = false;
            });
        }

        // Очистка всех категорий
        private void ClearAll()
        {
            Desserts.Clear(); MaleCakes.Clear(); FemaleCakes.Clear();
            KidsCakes.Clear(); WeddingCakes.Clear(); Sets.Clear(); Drinks.Clear();
        }

        // Нажатие на товар
        async void OpenProduct(Product product)
        {
            await Shell.Current.GoToAsync(nameof(ProductPage), new Dictionary<string, object>
            {
                ["Product"] = product
            });
        }
    }
}