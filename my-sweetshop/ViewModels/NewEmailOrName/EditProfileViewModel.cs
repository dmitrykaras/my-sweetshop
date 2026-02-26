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

        private UserDto _originalUser;

        private string _newFirstname;
        private string _newLastname;

        private string _newEmail;

        private string _currentEmail;

        private readonly EmailCache _emailCache;

        // Флаг активности Cooldown
        private bool _cooldownActive;
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

        private int _cooldownSeconds = 120;

        // Текст для Cooldown 
        private string _cooldownText;
        public string CooldownText
        {
            get => _cooldownText;
            set => SetProperty(ref _cooldownText, value);
        }

        public string NewFirstname
        {
            get => _newFirstname;
            set => SetProperty(ref _newFirstname, value);
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
            set => SetProperty(ref _currentEmail, value);
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
            GoToChangeEmailCommand = new Command(async () => await GoToChangeEmail(), () => !_cooldownActive);
            BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Инициализация
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
        private async Task LoadUserAsync()
        {
            try
            {
                await _session.InitializeAsync();

                var user = await _userService.GetCurrentUser();

                NewFirstname = user.FirstName;
                NewLastname = user.LastName;

                // Выбираем первый не пустой вариант (если их почему-то несколько)
                var emailToShow = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : _session.Email;

                CurrentEmail = !string.IsNullOrWhiteSpace(emailToShow) ? emailToShow : "Почта не указана";
            }
            catch (Exception ex)
            {
                await ShowToast("Ошибка загрузки данных");
            }
        }

        // Метод сохранения данных
        private async Task SaveAsync()
        {
            if (IsBusy) return;

            // Простая валидация перед отправкой
            if (string.IsNullOrWhiteSpace(NewFirstname) || string.IsNullOrWhiteSpace(NewLastname))
            {
                await ShowToast("Имя и фамилия не могут быть пустыми");
                return;
            }

            // Сравниваем с оригиналом (опционально, но профессионально)
            if (NewFirstname == _originalUser?.FirstName && NewLastname == _originalUser?.LastName)
            {
                await ShowToast("Изменений не обнаружено");
                return;
            }

            IsBusy = true;
            ((Command)SaveCommand).ChangeCanExecute(); // Обновляем состояние кнопки

            try
            {
                var updateDto = new UpdateProfileDto
                {
                    FirstName = NewFirstname,
                    LastName = NewLastname
                };

                await _userService.UpdateProfileAsync(updateDto);

                    // Обновляем "оригинал", чтобы кнопка сохранения снова стала неактивной
                    _originalUser.FirstName = NewFirstname;
                    _originalUser.LastName = NewLastname;

                    await ShowToast("Профиль успешно обновлен");
            }
            catch (Exception ex)
            {
                // Логируем ошибку (ex)
                await ShowToast("Не удалось сохранить изменения");
            }
            finally
            {
                IsBusy = false;
                ((Command)SaveCommand).ChangeCanExecute();
            }
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
