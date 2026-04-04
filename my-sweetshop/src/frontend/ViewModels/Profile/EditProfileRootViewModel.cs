using my_sweetshop.ViewModels.Profile.NewEmailOrName;

namespace my_sweetshop.ViewModels.Profile
{
    public class EditProfileRootViewModel : BaseViewModel
    {
        public UserProfileViewModel Profile { get; }
        public UserPointsViewModel Points { get; }

        public EditProfileRootViewModel(UserProfileViewModel profile, UserPointsViewModel points)
        {
            Profile = profile;
            Points = points;
        }
    }
}
