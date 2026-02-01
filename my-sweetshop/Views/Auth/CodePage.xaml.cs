using my_sweetshop.Services;

namespace my_sweetshop.Views.Auth;

public partial class CodePage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly AuthSession _session;

    private readonly string _email;

    private int _cooldownSeconds = 120;
    private bool _cooldownActive;

    public CodePage(AuthApi authApi, AuthSession session, string email)
    {
        InitializeComponent();
        _authApi = authApi;
        _session = session;
        _email = email;

        HintLabel.Text = $"На почту {_email} отправлено письмо с кодом подтверждения";

        StartCooldown();
    }

    private async void OnBack(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnResendTapped(object sender, EventArgs e)
    {
        if (_cooldownActive) return;

        try
        {
            await _authApi.RequestCodeAsync(_email);
            await DisplayAlert("Готово", "Код отправлен повторно. Проверьте “Спам”.", "Ок");
            StartCooldown();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "Ок");
        }
    }

    private void OnDigitChanged(object sender, TextChangedEventArgs e)
    {
        // только цифры
        if (!string.IsNullOrEmpty(e.NewTextValue) && !char.IsDigit(e.NewTextValue[0]))
        {
            ((Entry)sender).Text = "";
            return;
        }

        // авто-переход по полям
        if (sender == D1 && D1.Text?.Length == 1) D2.Focus();
        else if (sender == D2 && D2.Text?.Length == 1) D3.Focus();
        else if (sender == D3 && D3.Text?.Length == 1) D4.Focus();

        TryVerifyIfComplete();
    }

    private async void TryVerifyIfComplete()
    {
        var code = $"{D1.Text}{D2.Text}{D3.Text}{D4.Text}";
        if (code.Length != 4 || code.Any(c => !char.IsDigit(c)))
            return;

        // чтобы не дёргалось
        D1.IsEnabled = D2.IsEnabled = D3.IsEnabled = D4.IsEnabled = false;

        try
        {
            var resp = await _authApi.VerifyCodeAsync(_email, code);

            await _session.SetTokenAsync(resp.Token);

            if (resp.NeedsProfile)
                await Navigation.PushAsync(new ProfilePage());
            else
                Application.Current.Windows[0].Page = MauiProgram.ServiceProvider.GetService<MainPage>()!;
        }
        catch
        {
            ShowCodeError();
            ClearCode();
            D1.Focus();
        }
        finally
        {
            D1.IsEnabled = D2.IsEnabled = D3.IsEnabled = D4.IsEnabled = true;
        }
    }

    private void ShowCodeError()
    {
        ErrorLabel.IsVisible = true;
        Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () =>
        {
            ErrorLabel.IsVisible = false;
        });
    }

    private void ClearCode()
    {
        D1.Text = D2.Text = D3.Text = D4.Text = "";
    }

    private void StartCooldown()
    {
        _cooldownActive = true;
        _cooldownSeconds = 120;

        ResendLabel.Opacity = 0.4;
        CooldownLabel.Text = $"Повторная отправка через {_cooldownSeconds} сек.";

        this.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            _cooldownSeconds--;
            CooldownLabel.Text = $"Повторная отправка через {_cooldownSeconds} сек.";

            if (_cooldownSeconds <= 0)
            {
                _cooldownActive = false;
                ResendLabel.Opacity = 1;
                CooldownLabel.Text = "";
                return false;
            }

            return true;
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        D1.Focus();
    }

}