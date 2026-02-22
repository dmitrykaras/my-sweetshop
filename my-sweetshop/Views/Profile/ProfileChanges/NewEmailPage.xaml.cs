using my_sweetshop.ViewModels.NewEmailOrName;
namespace my_sweetshop.Views.Profile.ProfileChanges;

public partial class NewEmailPage : ContentPage
{
    // Флаг для очистки полей кода, чтобы не срабатывали события OnDigitChanged
    private bool _isClearing;

    // VM, к которому привязан Page
    private NewEmailViewModel VM => (NewEmailViewModel)BindingContext;

    public NewEmailPage(NewEmailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        _email = vm.Email;
    }

    // Шаг на прошлую страницу
    void OnBack(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("..");
    }

    // Автопереход фокуса между Entry и базовая валидация
    private void OnDigitChanged(object sender, TextChangedEventArgs e)
    {
        if (_isClearing) return;

        // Если включён cooldown на повторный ввод, очищаем поле
        if (VM.VerifyCooldownActive)
        {
            ((Entry)sender).Text = "";
            return;
        }

        // Только цифры
        if (!string.IsNullOrEmpty(e.NewTextValue) && !char.IsDigit(e.NewTextValue[0]))
        {
            ((Entry)sender).Text = "";
            return;
        }

        // Авто-переход по полям
        if (sender == D1 && D1.Text?.Length == 1) D2.Focus();
        else if (sender == D2 && D2.Text?.Length == 1) D3.Focus();
        else if (sender == D3 && D3.Text?.Length == 1) D4.Focus();

        // Попытка верификации, если код полностью введён
        TryVerify();
    }

    // Попытка верификации кода через VM
    private async void TryVerify()
    {
        if (VM.VerifyCooldownActive) return;

        var code = $"{D1.Text}{D2.Text}{D3.Text}{D4.Text}";
        if (code.Length != 4 || code.Any(c => !char.IsDigit(c)))
            return;

        // Блокируем ввод, пока идёт проверка
        DisableInputs();

        var result = await VM.VerifyCodeAsync(code);

        // Если ошибка — показываем и очищаем поля
        if (!result.Success)
        {
            ShowError(result.ErrorMessage);
            ClearCode();
            D1.Focus();
        }

        // Разблокируем поля, если нет cooldown
        if (!VM.VerifyCooldownActive)
            EnableInputs();
    }

    // Блокируем все Entry (защита от DDOS/многократного ввода)
    private void DisableInputs()
    {
        D1.IsEnabled = D2.IsEnabled = D3.IsEnabled = D4.IsEnabled = false;
    }

    // Разблокировка полей
    private void EnableInputs()
    {
        D1.IsEnabled = D2.IsEnabled = D3.IsEnabled = D4.IsEnabled = true;
    }

    // Очистка полей кода (с флагом _isClearing, чтобы не срабатывали события)
    private void ClearCode()
    {
        _isClearing = true;
        D1.Text = D2.Text = D3.Text = D4.Text = "";
        _isClearing = false;
    }

    // Обработка повторной отправки кода через VM
    private async void OnResendTapped(object sender, EventArgs e)
    {
        var ok = await VM.ResendCodeAsync();
        if (ok)
            StartResendCooldown();
    }

    // UI Кулдаун на повторную отправку
    private int _cooldownSeconds = 120;
    private bool _cooldownActive;
    private string _email;

    private void StartResendCooldown()
    {
        _cooldownActive = true;
        _cooldownSeconds = 120;

        // Полупрозрачная кнопка пока кулдаун активен
        ResendLabel.Opacity = 0.4;
        CooldownLabel.Text = $"Повторная отправка через {_cooldownSeconds} сек.";

        // Таймер визуального кулдауна
        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            _cooldownSeconds--;
            CooldownLabel.Text = $"Повторная отправка через {_cooldownSeconds} сек.";

            if (_cooldownSeconds <= 0)
            {
                _cooldownActive = false;
                ResendLabel.Opacity = 1;
                CooldownLabel.Text = "";
                return false; // остановка таймера
            }

            return true; // продолжение таймера
        });
    }

    // Вывод ошибки в Label
    private void ShowError(string text)
    {
        ErrorLabel.Text = text;
        ErrorLabel.IsVisible = true;
    }

    // Жизненный цикл Page: ставим фокус на первый Entry
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!VM.VerifyCooldownActive)
            D1.Focus();
    }
}