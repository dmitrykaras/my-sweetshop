namespace my_sweetshop.Views.Catalog;

public partial class CatalogPage : ContentPage
{
    public CatalogPage()
    {
        InitializeComponent();
    }

    private async void OnOpenProductClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProductPage));
    }
}