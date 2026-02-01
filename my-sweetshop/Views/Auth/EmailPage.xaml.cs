using my_sweetshop.Services;

namespace my_sweetshop.Views.Auth;

public partial class EmailPage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly CodePageFactory _codePageFactory;

    public EmailPage(AuthApi authApi, CodePageFactory codePageFactory)
    {
        InitializeComponent();
        _authApi = authApi;
        _codePageFactory = codePageFactory;
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

            var authApi = MauiProgram.ServiceProvider.GetService<AuthApi>();
            var session = MauiProgram.ServiceProvider.GetService<AuthSession>();

            if (authApi == null || session == null || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Ошибка", "Невозможно продолжить: сервис не найден или email пустой", "Ок");
                return;
            }

            var codePage = new CodePage(authApi, session, email);
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
