using my_sweetshop.ViewModels.NewEmail;

namespace my_sweetshop.Views.Profile;

public partial class EditProfilePage : ContentPage
{
	public EditProfilePage(EditProfileViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}