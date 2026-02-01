using my_sweetshop.Services;

namespace my_sweetshop.Views.Auth;

public partial class EmailPage : ContentPage
{
    private readonly AuthApi _authApi;

    public EmailPage(AuthApi authApi)
    {
        InitializeComponent();
        _authApi = authApi;
    }

    private async void OnClose(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnContinue(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            await DisplayAlert("Ошибка", "Введите корректный email", "Ок");
            return;
        }

        ContinueButton.IsEnabled = false;

        try
        {
            // отправляем код на почту
            await _authApi.RequestCodeAsync(email);

            // переходим на страницу ввода кода
            var codePage = new CodePage(
                MauiProgram.ServiceProvider.GetService<AuthApi>()!,
                MauiProgram.ServiceProvider.GetService<AuthSession>()!,
                email
            );

            await Navigation.PushAsync(codePage);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "Ок");
        }
        finally
        {
            ContinueButton.IsEnabled = true;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EmailEntry.Focus(); // ставим фокус, клавиатура появится на Android/iOS
    }

}
