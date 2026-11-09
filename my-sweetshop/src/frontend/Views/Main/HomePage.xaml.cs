using my_sweetshop.ViewModels;

namespace my_sweetshop.Views.Main
{
    public partial class HomePage : ContentPage
    {
        private readonly QrCardViewModel _qr =
            MauiProgram.ServiceProvider.GetRequiredService<QrCardViewModel>();
        public HomePage()
        {
            InitializeComponent();
            QrCard.BindingContext = _qr;
        }

        private async void OnCardTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement v)
            {
                await v.ScaleToAsync(0.95, 80);
                await v.ScaleToAsync(1, 80);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _qr.ActivateAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _qr.Deactivate();
        }
    }
}