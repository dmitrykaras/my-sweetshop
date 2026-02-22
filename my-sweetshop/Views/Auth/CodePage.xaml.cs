using my_sweetshop.Services.Api;
using my_sweetshop.Services.AuthStep;

namespace my_sweetshop.Views.Auth;

public partial class CodePage : ContentPage
{
    private readonly AuthApi _authApi;
    private readonly AuthSession _session;
    private readonly string _email;
    private bool _isClearing;

    // resend cooldown
    private int _cooldownSeconds = 120;
    private bool _cooldownActive;

    // verify cooldown
    private int _verifyRetrySeconds;
    private bool _verifyCooldownActive;

    public CodePage(AuthApi authApi, AuthSession session, string email)
    {
        InitializeComponent();
        _authApi = authApi;
        _session = session;
        _email = email;

        HintLabel.Text = $"На почту {_email} отправлено письмо с кодом подтверждения";

        StartResendCooldown();
    }

    private async void OnBack(object sender, EventArgs e) => await Navigation.PopAsync();

    private async void OnResendTapped(object sender, EventArgs e)
    {
        if (_cooldownActive) return;

        try
        {
            await _authApi.RequestCodeAsync(_email);
            await DisplayAlertAsync("Готово", "Код отправлен повторно. Проверьте “Спам”.", "Ок");
            StartResendCooldown();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", ex.Message, "Ок");
        }
    }

    private void OnDigitChanged(object sender, TextChangedEventArgs e)
    {
        if (_isClearing) return;

        if (_verifyCooldownActive)
        {
            ((Entry)sender).Text = "";
            return;
        }

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
        if (_verifyCooldownActive) return;

        var code = $"{D1.Text}{D2.Text}{D3.Text}{D4.Text}";
        if (code.Length != 4 || code.Any(c => !char.IsDigit(c)))
            return;

        SetInputsEnabled(false);

        try
        {
            var resp = await _authApi.VerifyCodeAsync(_email, code);

            await _session.SetTokenAsync(resp.Token);
            await SecureStorage.SetAsync("access_token", resp.Token);

            await SecureStorage.SetAsync("refresh_token", resp.RefreshToken);

            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                if (resp.NeedsProfile)
                    window.Page = new NavigationPage(new CompletionProfilePage(_email));
                else
                    window.Page = MauiProgram.ServiceProvider.GetRequiredService<AppShell>();
            }
        }
        catch (ApiException apiEx)
        {
            HandleVerifyApiError(apiEx);
            ClearCode();
            D1.Focus();
        }
        catch
        {
            ShowError("Неверный код");
            ClearCode();
            D1.Focus();
        }
        finally
        {
            if (!_verifyCooldownActive)
                SetInputsEnabled(true);
        }
    }

    private void HandleVerifyApiError(ApiException apiEx)
    {
        var err = apiEx.Error;

        if (err == null)
        {
            ShowError(apiEx.Message);
            return;
        }

        // 1) неверный код
        if (err.Error == "invalid_code")
        {
            var attemptsLeft = err.AttemptsLeft ?? 0;
            ShowError($"Неверный код. Осталось попыток: {attemptsLeft}");
            return;
        }

        // 2) попытки закончились
        if (err.Error == "too_many_attempts")
        {
            var seconds = err.RetryAfterSeconds ?? 0;
            StartVerifyCooldown(seconds);
            ShowError("Попытки закончились");
            return;
        }

        // остальное
        ShowError(err.Message);
    }

    private void ShowError(string text)
    {
        ErrorLabel.Text = text;
        ErrorLabel.IsVisible = true;
    }

    private void ClearCode()
    {
        _isClearing = true;
        D1.Text = D2.Text = D3.Text = D4.Text = "";
        _isClearing = false;
    }

    private void SetInputsEnabled(bool enabled)
    {
        D1.IsEnabled = D2.IsEnabled = D3.IsEnabled = D4.IsEnabled = enabled;
    }

    // Инициализация отсчёта повтроной отправки кода
    private void StartResendCooldown()
    {
        _cooldownActive = true;
        _cooldownSeconds = 120;

        ResendLabel.Opacity = 0.4;
        CooldownLabel.Text = $"Повторная отправка через {_cooldownSeconds} сек.";

        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
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

    private void StartVerifyCooldown(int seconds)
    {
        if (seconds <= 0)
        {
            ShowError("Попробуйте позже");
            return;
        }

        _verifyCooldownActive = true;
        _verifyRetrySeconds = seconds;

        // блокируем ввод
        SetInputsEnabled(false);

        VerifyCooldownLabel.IsVisible = true;
        VerifyCooldownLabel.Text = $"Повторный ввод кода через {_verifyRetrySeconds} сек.";

        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            _verifyRetrySeconds--;

            VerifyCooldownLabel.Text = $"Повторный ввод кода через {_verifyRetrySeconds} сек.";

            if (_verifyRetrySeconds <= 0)
            {
                _verifyCooldownActive = false;

                VerifyCooldownLabel.IsVisible = false;
                VerifyCooldownLabel.Text = "";

                ClearCode();
                SetInputsEnabled(true);
                D1.Focus();

                return false;
            }

            return true;
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_verifyCooldownActive)
            D1.Focus();
    }
}