using my_sweetshop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace my_sweetshop.ViewModels;

public class HomeViewModel : BaseViewModel
{
    public ObservableCollection<PurchaseHistoryItem> PurchaseHistory { get; } =
        new();

    private bool _isHistoryExpanded;
    public bool IsHistoryExpanded
    {
        get => _isHistoryExpanded;
        set => SetProperty(ref _isHistoryExpanded, value);
    }

    public string HistoryToggleText =>
        IsHistoryExpanded ? "Свернуть" : "Развернуть";

    public ICommand ToggleHistoryCommand { get; }
    public ICommand OpenStoresCommand { get; }
    public ICommand OrderDeliveryCommand { get; }

    public HomeViewModel()
    {
        ToggleHistoryCommand = new Command(() =>
        {
            IsHistoryExpanded = !IsHistoryExpanded;
            OnPropertyChanged(nameof(HistoryToggleText));
        });

        OpenStoresCommand = new Command(() =>
        {
            // позже: навигация к карте
        });

        OrderDeliveryCommand = new Command(() =>
        {
            // позже: переход к доставке
        });

        // временные данные
        PurchaseHistory.Add(new PurchaseHistoryItem("Торт Наполеон", "1200 ₽"));
        PurchaseHistory.Add(new PurchaseHistoryItem("Эклеры (4 шт)", "480 ₽"));

        UpdateIsLastFlags();
    }

    //функция, для того, чтобы не ставить в конце разделитель
    private void UpdateIsLastFlags()
    {
        for (int i = 0; i < PurchaseHistory.Count; i++)
            PurchaseHistory[i].IsLast = i == PurchaseHistory.Count - 1;
    }
}