namespace my_sweetshop.Views.Main;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnCardPressed(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement view)
            await view.ScaleTo(0.96, 80, Easing.CubicOut);
    }

    private async void OnCardReleased(object sender, PointerEventArgs e)
    {
        if (sender is VisualElement view)
            await view.ScaleTo(1, 80, Easing.CubicOut);
    }

}