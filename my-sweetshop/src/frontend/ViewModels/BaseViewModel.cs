using CommunityToolkit.Mvvm.ComponentModel;

namespace my_sweetshop.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        // Вместо приватных полей объявляем публичные partial свойства
        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;
    }
}