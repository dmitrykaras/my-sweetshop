using System.Collections.ObjectModel;
using System.Windows.Input;
using my_sweetshop.Models;
using my_sweetshop.Views.Catalog;

namespace my_sweetshop.ViewModels
{
    [QueryProperty(nameof(CategoryKey), "Category")]
    public class CategoryViewModel
    {
        public ObservableCollection<Product> Products { get; } = new();

        public ICommand OpenProductCommand { get; }

        private string _categoryKey;
        public string CategoryKey
        {
            get => _categoryKey;
            set
            {
                _categoryKey = value;
                LoadProducts();
            }
        }

        public string Title => GetCategoryTitle(CategoryKey);

        public CategoryViewModel()
        {
            OpenProductCommand = new Command<Product>(OpenProduct);
        }

        void LoadProducts()
        {
            Products.Clear();

            // mock-данные, потом будет сервис / API
            if (CategoryKey == "Desserts")
            {
                Products.Add(new Product
                {
                    Id = "1",
                    Name = "Шоколадный торт",
                    Image = "cake1.png",
                    Description = "Насыщенный шоколадный вкус"
                });

                Products.Add(new Product
                {
                    Id = "2",
                    Name = "Эклер",
                    Image = "eclair.png",
                    Description = "Классический французский десерт"
                });
            }
        }

        async void OpenProduct(Product product)
        {
            await Shell.Current.GoToAsync(
                nameof(ProductPage),
                new Dictionary<string, object>
                {
                    ["Product"] = product
                });
        }

        string GetCategoryTitle(string key) => key switch
        {
            "Desserts" => "ДЕСЕРТЫ",
            "MaleCakes" => "МУЖСКИЕ ТОРТЫ",
            "FemaleCakes" => "ЖЕНСКИЕ ТОРТЫ",
            "KidsCakes" => "ДЕТСКИЕ ТОРТЫ",
            "WeddingCakes" => "СВАДЕБНЫЕ ТОРТЫ",
            "Sets" => "НАБОРЫ",
            "Drinks" => "НАПИТКИ",
            _ => key
        };
    }
}