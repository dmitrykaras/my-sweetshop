using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Services.UserService;
using my_sweetshop.Views.Profile.ProfileChanges;
using System.Windows.Input;

namespace my_sweetshop.ViewModels.Profile.NewEmailOrName
{
    public class UserProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly EmailCache _emailCache;
        private readonly ChangeEmail _changeEmail;
        private readonly AuthSession _session;
        private readonly IProfileService _profileService;

        private UserModel _originalUser;
        private string _currentFirstname;
        private string _newFirstname;
        private string _currentLastname;
        private string _newLastname;
        private string _newEmail;
        private string _currentEmail;
        private bool _cooldownActive;
        private string _cooldownText;

        private int _cooldownSeconds = 120;

        public bool IsCooldownActive
        {
            get => _cooldownActive;
            set
            {
                if (SetProperty(ref _cooldownActive, value))
                {
                    // Это заставит кнопку перепроверить свою доступность (CanExecute)
                    ((Command)GoToChangeEmailCommand).ChangeCanExecute();
                }
            }
        }

        public string CooldownText
        {
            get => _cooldownText;
            set => SetProperty(ref _cooldownText, value);
        }

        public string CurrentFirstname
        {
            get => _currentFirstname;
            set => SetProperty(ref _currentFirstname, value);
        }

        public string NewFirstname
        {
            get => _newFirstname;
            set => SetProperty(ref _newFirstname, value);
        }

        public string CurrentLastname
        {
            get => _currentLastname;
            set => SetProperty(ref _currentLastname, value);
        }

        public string NewLastname
        {
            get => _newLastname;
            set => SetProperty(ref _newLastname, value);
        }

        public string NewEmail
        {
            get => _newEmail;
            set => SetProperty(ref _newEmail, value);
        }

        public string CurrentEmail
        {
            get => _currentEmail;
            set
            {
                if (_currentEmail != value)
                {
                    _currentEmail = value;
                    OnPropertyChanged();
                }
            } 
        }

        public ICommand SaveCommand { get; }
        public ICommand GoToChangeEmailCommand { get; }

        public UserProfileViewModel(IUserService userService, EmailCache emailCache, ChangeEmail changeEmail,
            AuthSession session, IProfileService profileService)
        {
            _userService = userService;
            _emailCache = emailCache;
            _changeEmail = changeEmail;
            _session = session;
            _profileService = profileService;

            SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
            GoToChangeEmailCommand = new Command(async () => await GoToChangeEmail(), () => !IsCooldownActive);

            InitializeViewModel();
        }

        // Инициализация VM
        private async void InitializeViewModel()
        {
            IsBusy = true;
            await LoadUserAsync();
            IsBusy = false;
        }

        // Метод загрузки данных
        public async Task LoadUserAsync()
        {
            try
            {
                await _session.InitializeAsync();

                // 1. Получаем данные
                var user = await _userService.GetCurrentUser();

                // 2. ПРОВЕРКА: если user пришел null, не идем дальше
                if (user == null)
                {
                    await ShowToast("Данные пользователя не найдены");
                    return;
                }

                // 3. Сохраняем в оригинальный объект
                _originalUser = user;

                // 4. Безопасно заполняем поля
                CurrentFirstname = _originalUser.FirstName;
                CurrentLastname = _originalUser.LastName;


                var emailToShow = !string.IsNullOrWhiteSpace(_originalUser.Email)
                                  ? _originalUser.Email
                                  : _session.Email;

                CurrentEmail = !string.IsNullOrWhiteSpace(emailToShow)
                               ? emailToShow
                               : "Почта не указана";
            }
            catch (Exception ex)
            {
                // Логируйте ex, чтобы видеть реальную причину (ошибка сети, 401 и т.д.)
                await ShowToast("Ошибка загрузки данных");
            }
        }

        // Метод сохранения данных
        private async Task SaveAsync()
        {
            if (IsBusy) return;

            var _currentUser = await _profileService.GetProfileAsync();

            // Валидация
            if (string.IsNullOrWhiteSpace(NewFirstname) && string.IsNullOrWhiteSpace(NewLastname))
            {
                await ShowToast("Заполните все поля");
                return;
            }

            // Проверка на изменения
            if (NewFirstname == _currentUser?.FirstName && NewLastname == _currentUser?.LastName)
            {
                await ShowToast("Изменений нет");
                return;
            }

            IsBusy = true;
            try
            {
                var dto = new UpdateProfileDto
                {
                    FirstName = NewFirstname,
                    LastName = NewLastname
                };

                bool success = await _profileService.UpdateProfileAsync(dto);

                if (success)
                {
                    await ShowToast("Профиль обновлен");
                    await LoadUserAsync();
                    ClearEntryString();
                }
                else
                {
                    await ShowToast("Ошибка при сохранении");
                }
            }
            catch (Exception)
            {
                await ShowToast("Ошибка сети");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Очистка полей ввода имени и фамилии 
        private void ClearEntryString()
        {
            // Очищаем данные

            if (NewFirstname != null) NewFirstname = "";
            if (NewLastname != null) NewLastname = "";
        }

        // Переход к странице ввода кода врификации и отправка кода
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

                // если запрос прошёл успешно, то запускаем таймер на cooldown
                await StartCooldownTimer();
            }
            catch
            {
                await ShowToast("Ошибка отправки кода или слишком много попыток");
            }
        }

        // Метод для отсчёта времени Cooldown
        private async Task StartCooldownTimer()
        {
            IsCooldownActive = true;

            for (int i = _cooldownSeconds; i > 0; i--)
            {
                CooldownText = $"Повтор через {i}с";

                // Просто ждем 1 секунду, не блокируя поток
                await Task.Delay(1000);
            }

            IsCooldownActive = false;
            CooldownText = string.Empty;
        }

        // Метод вывода
        async Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            await toast.Show();
        }
    }
}