using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using my_sweetshop.Dtos;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Domain;
using my_sweetshop.Views.Profile.ProfileChanges;
using System.Windows.Input;

namespace my_sweetshop.ViewModels.NewEmail
{
    public class EditProfileViewModel : BaseViewModel
    {
        private readonly AuthSession _session;
        private readonly ChangeEmail _changeEmail;

        private string _newName;
        private string _newSurname;
        private string _newEmail;

        private readonly EmailCache _emailCache;

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

        public EditProfileViewModel(IUserService userService, EmailCache emailCache, ChangeEmail changeEmail, AuthSession session)
        {
            _userService = userService;
            _changeEmail = changeEmail;
            _emailCache = emailCache;
            _session = session;

            SaveCommand = new Command(async () => await SaveAsync());
            GoToChangeEmailCommand = new Command(async () => await GoToChangeEmail());
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // вызываем асинхронный метод для загрузки пользователя
            _ = LoadUserAsync();
            
        }

        private async Task LoadUserAsync()
        {
            await _session.InitializeAsync();
            var user = await _userService.GetCurrentUser();
            NewName = user.Name;
            NewSurname = user.Surname;
            NewEmail = user.Email;

            _session.Email ??= user.Email; // на случай, если SecureStorage пуст
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
                await ShowToast("Введите корректную новую почту");
                return;
            }

            try
            {
                // сохраняем новую почту
                _emailCache.TempEmail = NewEmail;

                // используем текущую почту пользователя
                var currentEmail = _session.Email ?? NewEmail; // fallback

                if (string.IsNullOrWhiteSpace(currentEmail))
                {
                    await ShowToast("Текущая почта не найдена");
                    return;
                }

                // отправка кода
                await _changeEmail.RequestCodeAsync(currentEmail, NewEmail);

                // переход на страницу ввода кода
                await Shell.Current.GoToAsync(nameof(NewEmailPage));
            }
            catch
            {
                await ShowToast("Ошибка отправки кода");
            }
        }

        async Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            await toast.Show();
        }
    }
}
