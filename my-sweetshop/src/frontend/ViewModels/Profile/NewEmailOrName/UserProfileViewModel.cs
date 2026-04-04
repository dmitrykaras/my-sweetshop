using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Services.UserService;
using my_sweetshop.Views.Profile.ProfileChanges;
using System.Windows.Input;
using System.Timers;
using System.ComponentModel.DataAnnotations;

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

        // Поля для смены почты
        private bool _isEmailCooldownActive;
        private string _emailCooldownText;
        private int _emailCooldownSeconds = 300;

        // Поля для сохранения профиля (имя/фамилия)
        private bool _isProfileCooldownActive;
        private string _profileCooldownText;
        private int _profileCooldownSeconds = 120;

        private DateTime? _emailCooldownEndTime;
        private DateTime? _profileCooldownEndTime;

        /// <summary>
        /// Cooldown - для почты и для смены имени и/или фамилии
        /// </summary>

        // Свойства для почты
        public bool IsEmailCooldownActive
        {
            get => _isEmailCooldownActive;
            set { if (SetProperty(ref _isEmailCooldownActive, value)) ((Command)GoToChangeEmailCommand).ChangeCanExecute(); }
        }
        public string EmailCooldownText
        {
            get => _emailCooldownText;
            set => SetProperty(ref _emailCooldownText, value);
        }

        // Свойства для профиля
        public bool IsProfileCooldownActive
        {
            get => _isProfileCooldownActive;
            set { if (SetProperty(ref _isProfileCooldownActive, value)) ((Command)SaveCommand).ChangeCanExecute(); }
        }
        public string ProfileCooldownText
        {
            get => _profileCooldownText;
            set => SetProperty(ref _profileCooldownText, value);
        }

        public string CurrentFirstname
        {
            get => _currentFirstname;
            set => SetProperty(ref _currentFirstname, value);
        }

        public string NewFirstname
        {
            get => _newFirstname;
            set
            {
                if (SetProperty(ref _newFirstname, value))
                {
                    // Уведомляем SaveCommand, что нужно перепроверить CanExecute
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        public string CurrentLastname
        {
            get => _currentLastname;
            set => SetProperty(ref _currentLastname, value);
        }

        public string NewLastname
        {
            get => _newLastname;
            set
            {
                if (SetProperty(ref _newLastname, value))
                {
                    ((Command)SaveCommand).ChangeCanExecute();
                }
            }
        }

        [EmailAddress(ErrorMessage = "Некорректный формат Email")]
        public string NewEmail
        {
            get => _newEmail;
            set
            {
                if (SetProperty(ref _newEmail, value))
                {
                    ((Command)GoToChangeEmailCommand).ChangeCanExecute();
                }
            }
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

            SaveCommand = new Command(
                execute: async () => await SaveAsync(),
                canExecute: () => !IsBusy && CanSave() && !IsProfileCooldownActive
            );

            GoToChangeEmailCommand = new Command(
                execute: async () => await GoToChangeEmail(),
                canExecute: () => !IsEmailCooldownActive && IsValidEmail(NewEmail)
            );

            InitializeViewModel();
        }

        // Инициализация VM
        private async void InitializeViewModel()
        {
            IsBusy = true;
            await LoadUserAsync();
            IsBusy = false;
        }

        // Вспомогательный метод для валидации кнопки сохранения
        private bool CanSave()
        {
            // Проверяем, что хотя бы в одном поле есть 2 или более символов
            bool fnValid = !string.IsNullOrWhiteSpace(NewFirstname) && NewFirstname.Length >= 2;
            bool lnValid = !string.IsNullOrWhiteSpace(NewLastname) && NewLastname.Length >= 2;

            return fnValid || lnValid;
        }

        // Вспомогательный метод для валидации почты
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return new EmailAddressAttribute().IsValid(email);
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
                    _ = StartCooldown(_profileCooldownSeconds, (v) => IsProfileCooldownActive = v, (t) => ProfileCooldownText = t, false);
                    await LoadUserAsync();
                    ClearEntryString();
                }
            }
            catch (ApiException apiEx)
            {
                var message = apiEx.Error?.Message ?? apiEx.Message;
                await ShowToast(message);
            }
            catch (Exception)
            {
                await ShowToast("Ошибка сети или сервера");
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
            if (!IsValidEmail(NewEmail))
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
                await StartCooldown(_emailCooldownSeconds, (v) => IsEmailCooldownActive = v, (t) => EmailCooldownText = t, false);
            }
            catch
            {
                await ShowToast("Ошибка отправки кода или слишком много попыток");
            }
        }

        // Метод для отсчёта времени Cooldown
        private async Task StartCooldown(int seconds, Action<bool> setStatus, Action<string> setText, bool isEmail)
        {
            var endTime = DateTime.Now.AddSeconds(seconds);

            // Сохраняем время окончания
            if (isEmail) _emailCooldownEndTime = endTime;
            else _profileCooldownEndTime = endTime;

            setStatus(true);

            while (DateTime.Now < endTime)
            {
                var remaining = (endTime - DateTime.Now).TotalSeconds;
                setText($"Повтор через {Math.Ceiling(remaining)}с");
                await Task.Delay(1000);
            }

            setText(string.Empty);
            setStatus(false);
        }
            // Метод вывода
            async Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            await toast.Show();
        }
    }
}