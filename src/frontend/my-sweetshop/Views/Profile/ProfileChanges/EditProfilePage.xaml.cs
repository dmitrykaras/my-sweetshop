using my_sweetshop.ViewModels.Profile;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;

namespace my_sweetshop.Views.Profile;

public partial class EditProfilePage : ContentPage
{
    // Флаг для очистки полей кода, чтобы не срабатывали события 
    private bool _isClearing;
	public EditProfilePage(EditProfileRootViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}