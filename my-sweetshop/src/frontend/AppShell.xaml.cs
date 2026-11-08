using my_sweetshop.Views.Catalog;
using my_sweetshop.Views.Profile;
using my_sweetshop.Views.Profile.FavoritesProduct;
using my_sweetshop.Views.Profile.ProfileChanges;
using my_sweetshop.Views.Profile.Cashier;

namespace my_sweetshop
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(ProductPage), typeof(ProductPage));
            Routing.RegisterRoute(nameof(EditProfilePage), typeof(EditProfilePage));
            Routing.RegisterRoute(nameof(NewEmailPage), typeof(NewEmailPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
            Routing.RegisterRoute(nameof(CashierPage), typeof(CashierPage));
            Routing.RegisterRoute(nameof(CashierSessionPage), typeof(CashierSessionPage));
        }
    }
}