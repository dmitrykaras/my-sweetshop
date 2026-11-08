using BarcodeScanning;
using my_sweetshop.Services.Api;
using my_sweetshop.ViewModels;

namespace my_sweetshop.Views.Profile.Cashier;

public partial class CashierPage : ContentPage
{
    private readonly LoyaltyApi _api;
    private int _handling;

    public CashierPage(LoyaltyApi api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _handling = 0;

        var granted = await Methods.AskForRequiredPermissionAsync();
        if (!granted)
        {
            await DisplayAlertAsync("Камера", "Разрешите доступ к камере в настройках телефона", "OK");
            return;
        }
        Camera.CameraEnabled = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Camera.CameraEnabled = false;
    }

    private async void OnDetected(object? sender, OnDetectionFinishedEventArg e)
    {
        var raw = e.BarcodeResults?.FirstOrDefault()?.DisplayValue;
        if (string.IsNullOrEmpty(raw) || !raw.StartsWith(QrCardViewModel.Prefix)) return;

        // Один скан за раз: камера присылает событие много раз подряд
        if (Interlocked.Exchange(ref _handling, 1) == 1) return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            Busy.IsVisible = Busy.IsRunning = true;
            Camera.CameraEnabled = false;
            try
            {
                var session = await _api.ScanAsync(raw[QrCardViewModel.Prefix.Length..]);
                await Shell.Current.GoToAsync(nameof(CashierSessionPage),
                    new ShellNavigationQueryParameters { ["Session"] = session });
            }
            catch (LoyaltyApiException ex)
            {
                await DisplayAlertAsync("Ошибка", ex.Message, "OK");
                Camera.CameraEnabled = true;
                Interlocked.Exchange(ref _handling, 0);
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Ошибка", "Нет связи с сервером", "OK");
                Camera.CameraEnabled = true;
                Interlocked.Exchange(ref _handling, 0);
            }
            finally
            {
                Busy.IsVisible = Busy.IsRunning = false;
            }
        });
    }
}
