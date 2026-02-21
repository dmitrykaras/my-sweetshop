using my_sweetshop.Views.Profile.ProfileChanges;
using System.Windows.Input;
using my_sweetshop.Services.Domain;
using my_sweetshop.Dtos;

namespace my_sweetshop.ViewModels.NewEmail
{
    public class EditProfileViewModel : BaseViewModel
    {
        private string _newName;
        private string _newSurname;
        private string _newEmail;

        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        public string NewSurname
        {
            get => _newSurname;
            set => SetProperty(ref _newSurname, value);
        }

        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand GoToChangeEmailCommand { get; }
        public ICommand BackCommand { get; }

        private readonly IUserService _userService;

        public EditProfileViewModel(IUserService userService)
        {
            _userService = userService;

            SaveCommand = new Command(async () => await SaveAsync());
            GoToChangeEmailCommand = new Command(async () => await GoToChangeEmail());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // вызываем асинхронный метод для загрузки пользователя
            _ = LoadUserAsync();
        }

        private async Task LoadUserAsync()
        {
            var user = await _userService.GetCurrentUser(); // await нужен
            NewName = user.Name;
            NewSurname = user.Surname;
            NewEmail = user.Email;
        }

        private async Task SaveAsync()
        {
            var user = await _userService.GetCurrentUser();

            bool nameChanged = !string.IsNullOrWhiteSpace(NewName) && NewName != user.Name;
            bool surnameChanged = !string.IsNullOrWhiteSpace(NewSurname) && NewSurname != user.Surname;

            // если ничего не менялось — вообще ничего не отправляем
            if (!nameChanged && !surnameChanged)
                return;

            var updateDto = new UpdateProfileDto();

            if (nameChanged)
                updateDto.FirstName = NewName;

            if (surnameChanged)
                updateDto.LastName = NewSurname;

            await _userService.UpdateProfileAsync(updateDto);
        }

        private async Task GoToChangeEmail()
        {
            if (string.IsNullOrWhiteSpace(NewEmail) || !NewEmail.Contains("@"))
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Введите корректную почту", "Ок");
                return;
            }

            // сохраняем почту в memory storage
            EmailCache.TempEmail = NewEmail;

            await Shell.Current.GoToAsync(nameof(NewEmailPage));
        }
    }
}
