using my_sweetshop.Views.Catalog;

namespace my_sweetshop
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ProductPage), typeof(ProductPage));
        }
    }
}
