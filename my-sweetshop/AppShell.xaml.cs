using my_sweetshop.Views.Catalog;
using my_sweetshop.Views.Profile;
using my_sweetshop.Views.Profile.ProfileChanges;

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
        }
    }
}