using my_sweetshop.ViewModels.Profile;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;

namespace my_sweetshop.Views.Profile;

public partial class EditProfilePage : ContentPage
{
	public EditProfilePage(EditProfileRootViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}