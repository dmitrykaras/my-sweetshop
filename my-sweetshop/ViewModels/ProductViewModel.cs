using my_sweetshop.Models;

namespace my_sweetshop.ViewModels
{
    [QueryProperty(nameof(Product), "Product")]
    public class ProductViewModel
    {
        public Product? Product { get; set; }
    }
}