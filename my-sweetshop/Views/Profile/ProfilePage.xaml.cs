using Microsoft.Maui.Controls;
using my_sweetshop.ViewModels.NewEmail;
using my_sweetshop.Views.Contact;
using my_sweetshop.Views.Profile.Cashier;

namespace my_sweetshop.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private int _secretTapCount = 0;

    public ProfilePage(EditProfileViewModel vm)
    {
        InitializeComponent();

        BindingContext = vm;
    }

    // Метод перемещения на EditProfilePage
    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    // Метод перемещения на FavoritesPage
    //private async void OnFavoritesTapped(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync(nameof(FavoritesPage));
    //}

    // Метод перемещения на раздел "Связаться"
    private async void OnHelpTapped(object sender, EventArgs e)
    {
        var shell = Shell.Current;

        // Берём TabBar (первый ShellItem)
        var tabBar = shell.Items.FirstOrDefault();
        if (tabBar == null) return;

        // Ищем секцию с Title = "Связаться"
        var contactSection = tabBar.Items
            .FirstOrDefault(section => section.Title == "Связаться");

        if (contactSection != null)
        {
            shell.CurrentItem = tabBar; // активируем TabBar
            shell.CurrentItem.CurrentItem = contactSection; // активируем вкладку
        }
    }

    // Метод перемещения на сайт
    private async void OnSiteTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://mysweetshop.ru");
    }

    // Временная логика кассира (5 тапов)
    private async void OnCashierSecretTapped(object sender, EventArgs e)
    {
        _secretTapCount++;
        if (_secretTapCount >= 5)
        {
            _secretTapCount = 0;
            await Shell.Current.GoToAsync(nameof(CashierPage));
        }
    }
}