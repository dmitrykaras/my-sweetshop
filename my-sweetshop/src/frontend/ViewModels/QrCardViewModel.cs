using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using my_sweetshop.Services.Api;
using QRCoder;

namespace my_sweetshop.ViewModels;

/// <summary>Карточка со скидочным QR на главной. QR создаётся при входе на экран и живёт 120 секунд.</summary>
public partial class QrCardViewModel(LoyaltyApi api) : ObservableObject
{
    // Префикс отсекает чужие QR-коды на стороне сканера, не дёргая сервер
    public const string Prefix = "MSS1:";

    private IDispatcherTimer? _timer;
    private DateTimeOffset _expiresAt;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQr))]
    public partial ImageSource? QrImage { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool NeedsRefresh { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public bool HasQr => QrImage is not null;

    // Вызывается из HomePage.OnAppearing
    public async Task ActivateAsync()
    {
        // Если прежний QR ещё долго живёт — просто возобновляем таймер, не гасим его новым
        if (QrImage is not null && _expiresAt - DateTimeOffset.UtcNow > TimeSpan.FromSeconds(20))
        {
            StartTimer();
            return;
        }
        await LoadAsync();
    }

    public void Deactivate() => _timer?.Stop();

    [RelayCommand]
    private Task RefreshAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        NeedsRefresh = false;
        try
        {
            var r = await api.GenerateQrAsync();
            _expiresAt = r.ExpiresAt;
            QrImage = RenderQr(Prefix + r.Token);
            StartTimer();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"QR error: {ex.Message}");
            QrImage = null;
            NeedsRefresh = true;
            StatusText = "Не удалось получить QR-код";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static ImageSource RenderQr(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data).GetGraphic(12);
        return ImageSource.FromStream(() => new MemoryStream(png));
    }

    private void StartTimer()
    {
        _timer ??= Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick -= OnTick;
        _timer.Tick += OnTick;
        UpdateRemaining();
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e) => UpdateRemaining();

    private void UpdateRemaining()
    {
        var left = _expiresAt - DateTimeOffset.UtcNow;
        if (left <= TimeSpan.Zero)
        {
            _timer?.Stop();
            QrImage = null;
            NeedsRefresh = true;
            StatusText = "QR-код истёк";
            return;
        }
        StatusText = $"Действует ещё {(int)Math.Ceiling(left.TotalSeconds)} с";
    }
}