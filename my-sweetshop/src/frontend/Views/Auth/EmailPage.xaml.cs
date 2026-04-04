using my_sweetshop.Services.Api;
using my_sweetshop.Services.AuthStep;
using System.ComponentModel.DataAnnotations;

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
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlertAsync("", "Введите email", "");
            return;
        }

        var validationModel = new EmailValidationModel { Email = email };
        var validationContext = new ValidationContext(validationModel);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(validationModel, validationContext, validationResults, validateAllProperties: true);

        if (!isValid)
        {
            await DisplayAlertAsync("", "Некорректный формат email", "");
            return;
        }

        email = email.ToLower();

        ContinueButton.IsEnabled = false;

        try
        {
            // отправляем код на почту
            await _authApi.RequestCodeAsync(email);

            var authApi = MauiProgram.ServiceProvider.GetService<AuthApi>();
            var session = MauiProgram.ServiceProvider.GetService<AuthSession>();

            if (authApi == null || session == null || string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlertAsync("Ошибка", "Невозможно продолжить: сервис не найден или email пустой", "Ок");
                return;
            }

            var codePage = new CodePage(authApi, session, email);
            await Navigation.PushAsync(codePage);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок");
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

    public class EmailValidationModel
    {
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; }
    }
}
