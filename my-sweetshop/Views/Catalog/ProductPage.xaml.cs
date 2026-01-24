using my_sweetshop.Models;

namespace my_sweetshop.Views.Catalog
{
    [QueryProperty(nameof(Product), "Product")]
    public partial class ProductPage : ContentPage
    {
        public ProductPage()
        {
            InitializeComponent();
        }

        private Product _product;
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                BindingContext = value;
            }
        }
    }
}