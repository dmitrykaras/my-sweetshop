namespace my_sweetshop.Views.Main
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }


        private async void OnCardTapped(object sender, EventArgs e)
        {
            if (sender is VisualElement v)
            {
                await v.ScaleToAsync(0.95, 80);
                await v.ScaleToAsync(1, 80);
            }
        }
    }
}