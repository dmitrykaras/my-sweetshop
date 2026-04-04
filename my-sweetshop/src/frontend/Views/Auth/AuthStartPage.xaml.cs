namespace my_sweetshop.Views.Auth;

public partial class AuthStartPage : ContentPage
{
    public AuthStartPage()
    {
        InitializeComponent();
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var emailPage = MauiProgram.ServiceProvider.GetService<EmailPage>()!;
        await Navigation.PushAsync(emailPage);
    }
}