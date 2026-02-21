using my_sweetshop.Services.Domain;
using System.Windows.Input;

namespace my_sweetshop.ViewModels.NewEmailOrName
{
    public class NewEmailViewModel : BaseViewModel
    {
        public string Email { get; set; }

        public ICommand ConfirmCommand { get; }

        private readonly IUserService _userService;

        public NewEmailViewModel(IUserService userService)
        {
            _userService = userService;

            Email = EmailCache.TempEmail;

            ConfirmCommand = new Command(async () => await ConfirmAsync());
        }

        private async Task ConfirmAsync()
        {
            await _userService.ChangeEmailAsync(Email);
            EmailCache.TempEmail = null;

            await Shell.Current.GoToAsync("..");
        }
    }
}
