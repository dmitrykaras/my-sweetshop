using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using my_sweetshop.Dtos;
using my_sweetshop.Services.Api;
using System.Globalization;

namespace my_sweetshop.ViewModels.Profile.Cashier;

public enum SessionOverlay { None, Choice, Accrue, ConfirmFinish, Result }

/// <summary> Экран кассира после скана QR </summary>
public partial class CashierSessionViewModel(LoyaltyApi api) : BaseViewModel, IQueryAttributable
{
    private ScanResponse _session = null!;

    // Состояние оверлея
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverlayVisible), nameof(IsChoice), nameof(IsAccrue), nameof(IsConfirm), nameof(IsResult))]
    public partial SessionOverlay Overlay { get; set; } = SessionOverlay.Choice;

    public bool IsOverlayVisible => Overlay != SessionOverlay.None;
    public bool IsChoice => Overlay == SessionOverlay.Choice;
    public bool IsAccrue => Overlay == SessionOverlay.Accrue;
    public bool IsConfirm => Overlay == SessionOverlay.ConfirmFinish;
    public bool IsResult => Overlay == SessionOverlay.Result;

    // Данные клиента
    [ObservableProperty] public partial string CustomerName { get; set; } = string.Empty;
    [ObservableProperty] public partial int Points { get; set; }

    // Страница конвертации
    [ObservableProperty] public partial string OrderAmountText { get; set; } = string.Empty;
    [ObservableProperty] public partial string RedeemText { get; set; } = string.Empty;
    [ObservableProperty] public partial string HintText { get; set; } = string.Empty;
    [ObservableProperty] public partial string PayableText { get; set; } = string.Empty;

    // Меню "копим" 
    [ObservableProperty] public partial string AccrueAmountText { get; set; } = string.Empty;

    // Ошибки и результат 
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorText { get; set; } = string.Empty;
    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    [ObservableProperty] public partial string ResultTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string ResultBig { get; set; } = string.Empty;
    [ObservableProperty] public partial string ResultSub { get; set; } = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Session", out var value) && value is ScanResponse s)
        {
            _session = s;
            CustomerName = s.CustomerName;
            Points = s.Points;
            Overlay = SessionOverlay.Choice;
        }
    }

    partial void OnOrderAmountTextChanged(string value) => Recompute();
    partial void OnRedeemTextChanged(string value) => Recompute();

    // Команды 
    [RelayCommand]
    private void ChooseRedeem() { ErrorText = string.Empty; Overlay = SessionOverlay.None; }

    [RelayCommand]
    private void ChooseAccrue() { ErrorText = string.Empty; Overlay = SessionOverlay.Accrue; }

    [RelayCommand]
    private void BackToChoice() { ErrorText = string.Empty; Overlay = SessionOverlay.Choice; }

    // "Отменить" на странице конвертации -> снова меню "Списываем или копим?"
    [RelayCommand]
    private void CancelRedeem() => BackToChoice();

    // "Принять" -> проверяем локально и спрашиваем "Завершить расчёт скидки?"
    [RelayCommand]
    private void AcceptRedeem()
    {
        var error = ValidateRedeem(out _, out _);
        if (error is not null) { ErrorText = error; return; }
        ErrorText = string.Empty;
        Overlay = SessionOverlay.ConfirmFinish;
    }

    // "Нет" -> возвращаемся на страницу конвертации
    [RelayCommand]
    private void DeclineFinish() => Overlay = SessionOverlay.None;

    // "Да" -> сервер списывает баллы и возвращает сумму для терминала
    [RelayCommand]
    private async Task ConfirmFinishAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var error = ValidateRedeem(out var amount, out var points);
            if (error is not null) { ErrorText = error; Overlay = SessionOverlay.None; return; }

            var r = await api.RedeemAsync(_session.SessionId, amount, points);
            Points = r.NewBalance;
            ResultTitle = "Скидка применена";
            ResultBig = $"{Fmt(r.FinalAmount)} ₽";
            ResultSub = $"Введите в терминал только эту сумму.\nСписано баллов: {r.Redeemed}. Остаток у клиента: {r.NewBalance}";
            Overlay = SessionOverlay.Result;
        }
        catch (LoyaltyApiException ex) { ErrorText = ex.Message; Overlay = SessionOverlay.None; }
        catch (Exception) { ErrorText = "Нет связи с сервером. Повторите попытку"; Overlay = SessionOverlay.None; }
        finally { IsBusy = false; }
    }

    // "Копим": сумма заказа -> баллы клиенту
    [RelayCommand]
    private async Task SubmitAccrueAsync()
    {
        if (IsBusy) return;
        if (!TryParseAmount(AccrueAmountText, out var amount))
        {
            ErrorText = "Введите общую стоимость заказа (например, 1250 или 1250,50)";
            return;
        }

        IsBusy = true;
        ErrorText = string.Empty;
        try
        {
            var r = await api.AccrueAsync(_session.SessionId, amount);
            Points = r.NewBalance;
            ResultTitle = "Баллы начислены";
            ResultBig = $"+{r.Earned}";
            ResultSub = $"Баланс клиента: {r.NewBalance}";
            Overlay = SessionOverlay.Result;
        }
        catch (LoyaltyApiException ex) { ErrorText = ex.Message; }
        catch (Exception) { ErrorText = "Нет связи с сервером. Повторите попытку"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task DoneAsync() => await Shell.Current.GoToAsync("..");

    // Вспомогательное 
    private void Recompute()
    {
        ErrorText = string.Empty;
        PayableText = string.Empty;
        HintText = string.Empty;
        if (_session is null || !TryParseAmount(OrderAmountText, out var amount)) return;

        var max = Math.Min(_session.Points, (int)Math.Floor(amount * _session.MaxRedeemShare / _session.RubPerPoint));
        HintText = $"Можно списать до {max} баллов";

        if (int.TryParse(RedeemText, out var p) && p >= 0)
            PayableText = $"К оплате: {Fmt(amount - p * _session.RubPerPoint)} ₽";
    }

    private string? ValidateRedeem(out decimal amount, out int points)
    {
        points = 0;
        if (!TryParseAmount(OrderAmountText, out amount)) return "Введите общую стоимость заказа";
        if (!int.TryParse(RedeemText, out points) || points <= 0) return "Введите, сколько баллов списать";
        if (points > _session.Points) return "У клиента недостаточно баллов";

        var max = (int)Math.Floor(amount * _session.MaxRedeemShare / _session.RubPerPoint);
        if (points > max) return $"Можно списать не более {max} баллов";
        return null;
    }

    // Принимаем и "1250,50", и "1250.50"; не больше 2 знаков после запятой
    private static bool TryParseAmount(string? text, out decimal value) =>
        decimal.TryParse((text ?? string.Empty).Trim().Replace(',', '.'),
            NumberStyles.Number, CultureInfo.InvariantCulture, out value)
        && value > 0 && decimal.Round(value, 2) == value;

    private static string Fmt(decimal v) => v.ToString("0.##", CultureInfo.CurrentCulture);
}