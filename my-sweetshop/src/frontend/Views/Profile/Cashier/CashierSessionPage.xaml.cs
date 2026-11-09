using my_sweetshop.ViewModels.Profile.Cashier;

namespace my_sweetshop.Views.Profile.Cashier;

public partial class CashierSessionPage : ContentPage
{
    public CashierSessionPage(CashierSessionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
