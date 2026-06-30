using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using my_sweetshop.Dtos;
using my_sweetshop.Services.Api;
using my_sweetshop.Views.Profile;
using System.Windows.Input;

namespace my_sweetshop.ViewModels.Profile.NewEmailOrName;

public partial class NewEmailViewModel : BaseViewModel
{
    // Сервис для API
    private readonly AuthApi _authApi;
    private readonly ChangeEmail _changeEmail;
    private readonly AuthSession _session;
    private readonly EmailCache _emailCache;

    // Почта, на которую отправлен код
    public string Email { get; }

    // Кулдаун на повторный ввод кода
    public bool VerifyCooldownActive { get; private set; }

    // Генератор сам создаст VerifyCodeCommand и ResendCodeCommand.

    public NewEmailViewModel(ChangeEmail changeEmail, AuthSession session, AuthApi authApi, EmailCache emailCache)
    {
        _session = session;
        _changeEmail = changeEmail;
        _authApi = authApi;
        _emailCache = emailCache;

        // Берём временную почту из кеша
        Email = _session.Email!;

        // Сообщение пользователю о том, что код отправлен
        ShowToast($"На почту {Email} отправлено письмо с кодом подтверждения");
    }

    // Логика проверки кода
    [RelayCommand]
    public async Task<VerifyResult> VerifyCodeAsync(string code)
    {
        // Проверяем токен и временную почту перед началом операции
        if (string.IsNullOrEmpty(_session.Token))
            return VerifyResult.Fail("Сессия истекла. Пожалуйста, авторизуйтесь заново.");

        if (string.IsNullOrEmpty(_emailCache.TempEmail))
            return VerifyResult.Fail("Временный адрес почты не найден.");

        try
        {
            // меняем почту на новую
            await _changeEmail.VerifyCodeForChangeEmail(_emailCache.TempEmail, code, _session.Token);

            // обновляем сессию
            await _session.SetSessionAsync(_session.Token, _emailCache.TempEmail!);

            _emailCache.TempEmail = null;

            await Shell.Current.GoToAsync(nameof(ProfilePage));

            return VerifyResult.Successful();
        }
        catch (Exception ex)
        {
            return VerifyResult.Fail(ex.Message);
        }
    }

    // Попытка повторной отправки
    [RelayCommand]
    public async Task<bool> ResendCodeAsync()
    {
        try
        {
            await _changeEmail.RequestCodeAsync(this.Email, _emailCache.TempEmail!);
            ShowToast("Код отправлен повторно. Проверьте “Спам”.");
            return true;
        }
        catch
        {
            ShowToast("Ошибка повторной отправки");
            return false;
        }
    }

    // Вспомогательная функция для toast
    private void ShowToast(string text)
    {
        var toast = Toast.Make(text, ToastDuration.Short);
        toast.Show();
    }

    // Запись результата верификации
    public record VerifyResult(bool Success, string ErrorMessage)
    {
        public static VerifyResult Successful() => new(true, "");
        public static VerifyResult Fail(string msg) => new(false, msg);
    }
}