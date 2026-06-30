using CommunityToolkit.Mvvm.Input;
using my_sweetshop.ViewModels.Profile.NewEmailOrName;
using System;
using System.Threading.Tasks;

namespace my_sweetshop.ViewModels.Profile
{
    public partial class EditProfileRootViewModel : BaseViewModel
    {
        public UserProfileViewModel Profile { get; }
        public UserPointsViewModel Points { get; }

        // Таймер троттлинга для конкретно этой страницы
        private DateTime _lastUpdate = DateTime.MinValue;

        public EditProfileRootViewModel(UserProfileViewModel profile, UserPointsViewModel points)
        {
            Profile = profile;
            Points = points;
        }
    }
}