using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api;
using my_sweetshop.Services.Api.ProfileService;
using my_sweetshop.Services.UserService;
using my_sweetshop.Views.Profile.ProfileChanges;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace my_sweetshop.ViewModels.Profile.NewEmailOrName
{
    public partial class UserProfileViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly EmailCache _emailCache;
        private readonly ChangeEmail _changeEmail;
        private readonly AuthSession _session;
        private readonly IProfileService _profileService;

        private UserModel? _originalUser;
        private DateTime? _emailCooldownEndTime;
        private DateTime? _profileCooldownEndTime;
        private readonly int _emailCooldownSeconds = 300;
        private readonly int _profileCooldownSeconds = 120;

        // Атрибут [NotifyCanExecuteChangedFor] автоматически заставляет команду перепроверять свою доступность
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToChangeEmailCommand))]
        public partial bool IsEmailCooldownActive { get; set; }

        [ObservableProperty]
        public partial string EmailCooldownText { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        public partial bool IsProfileCooldownActive { get; set; }

        [ObservableProperty]
        public partial string ProfileCooldownText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CurrentFirstname { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        public partial string NewFirstname { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CurrentLastname { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        public partial string NewLastname { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(GoToChangeEmailCommand))]
        public partial string NewEmail { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string CurrentEmail { get; set; } = string.Empty;

        [ObservableProperty]
        public partial bool IsLoaded { get; set; }

        public UserProfileViewModel(IUserService userService, EmailCache emailCache, ChangeEmail changeEmail,
            AuthSession session, IProfileService profileService)
        {
            _userService = userService;
            _emailCache = emailCache;
            _changeEmail = changeEmail;
            _session = session;
            _profileService = profileService;

            InitializeViewModel();
        }

        private async void InitializeViewModel()
        {
            IsBusy = true;
            await LoadUserAsync();
            IsBusy = false;
        }

        private bool CanSave()
        {
            bool fnValid = !string.IsNullOrWhiteSpace(NewFirstname) && NewFirstname.Length >= 2;
            bool lnValid = !string.IsNullOrWhiteSpace(NewLastname) && NewLastname.Length >= 2;
            return fnValid || lnValid;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return new EmailAddressAttribute().IsValid(email);
        }

        // Генерируемые команды
        private bool CanExecuteSave() => !IsBusy && CanSave() && !IsProfileCooldownActive;

        // Сохранение изменений профиля
        [RelayCommand(CanExecute = nameof(CanExecuteSave))]
        private async Task SaveAsync()
        {
            if (IsBusy) return;

            var _currentUser = await _profileService.GetProfileAsync();

            if (string.IsNullOrWhiteSpace(NewFirstname) && string.IsNullOrWhiteSpace(NewLastname))
            {
                await ShowToast("Заполните все поля");
                return;
            }

            if (NewFirstname == _currentUser?.FirstName && NewLastname == _currentUser?.LastName)
            {
                await ShowToast("Изменений нет");
                return;
            }

            try
            {
                IsBusy = true;
                SaveCommand.NotifyCanExecuteChanged(); // Блокируем кнопку на время отправки

                var dto = new UpdateProfileDto
                {
                    FirstName = NewFirstname,
                    LastName = NewLastname
                };

                UserModel? updatedUser = await _profileService.UpdateProfileAsync(dto);

                if (updatedUser != null)
                {
                    CurrentFirstname = updatedUser.FirstName!;
                    CurrentLastname = updatedUser.LastName!;
                    _originalUser = updatedUser;

                    await ShowToast("Профиль обновлен");
                    _ = StartCooldown(_profileCooldownSeconds, (v) => IsProfileCooldownActive = v, (t) => ProfileCooldownText = t, false);
                    ClearEntryString();
                }
            }
            catch (ApiException apiEx)
            {
                await ShowToast(apiEx.Error?.Message ?? apiEx.Message);
            }
            catch (Exception)
            {
                await ShowToast("Ошибка сети или сервера");
            }
            finally
            {
                IsBusy = false;
                SaveCommand.NotifyCanExecuteChanged(); // Разблокируем кнопку
            }
        }

        private bool CanExecuteGoToChangeEmail() => !IsEmailCooldownActive && IsValidEmail(NewEmail);

        // Валидация и логика доступности почты для её смены
        [RelayCommand(CanExecute = nameof(CanExecuteGoToChangeEmail))]
        private async Task GoToChangeEmailAsync()
        {
            if (!IsValidEmail(NewEmail))
            {
                await ShowToast("Введите корректную новую почту");
                return;
            }

            try
            {
                _emailCache.TempEmail = NewEmail;
                var currentEmail = _session.Email ?? NewEmail;

                if (string.IsNullOrWhiteSpace(currentEmail))
                {
                    await ShowToast("Текущая почта не найдена");
                    return;
                }

                await _changeEmail.RequestCodeAsync(currentEmail, NewEmail);
                await Shell.Current.GoToAsync(nameof(NewEmailPage));
                _ = StartCooldown(_emailCooldownSeconds, (v) => IsEmailCooldownActive = v, (t) => EmailCooldownText = t, true);
            }
            catch
            {
                await ShowToast("Ошибка отправки кода или слишком много попыток");
            }
        }

        // Методы получения данных
        public async Task LoadUserAsync()
        {
            try
            {
                IsBusy = true;

                await _session.InitializeAsync();
                var user = await _userService.GetCurrentUser();

                if (user == null)
                {
                    await ShowToast("Данные пользователя не найдены");
                    return;
                }

                _originalUser = user;
                CurrentFirstname = _originalUser.FirstName!;
                CurrentLastname = _originalUser.LastName!;

                var emailToShow = !string.IsNullOrWhiteSpace(_originalUser.Email)
                                  ? _originalUser.Email
                                  : _session.Email;

                CurrentEmail = !string.IsNullOrWhiteSpace(emailToShow)
                               ? emailToShow
                               : "Почта не указана";
                IsLoaded = true;
            }
            catch (Exception)
            {
                await ShowToast("Ошибка загрузки данных");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Очистка полей имени и фамилии
        private void ClearEntryString()
        {
            NewFirstname = string.Empty;
            NewLastname = string.Empty;
        }

        // Очистка данных
        public void ClearData()
        {
            _originalUser = null;
            CurrentFirstname = string.Empty;
            CurrentLastname = string.Empty;
            CurrentEmail = string.Empty;
            NewFirstname = string.Empty;
            NewLastname = string.Empty;
            NewEmail = string.Empty;
            IsLoaded = false;
        }

        // Запуск кулдауна
        private async Task StartCooldown(int seconds, Action<bool> setStatus, Action<string> setText, bool isEmail)
        {
            var endTime = DateTime.Now.AddSeconds(seconds);

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

        // Всплывающие уведомления
        private async Task ShowToast(string text)
        {
            var toast = Toast.Make(text, ToastDuration.Short);
            await toast.Show();
        }

        // Команда для обновления, которая гарантированно выключит спиннер
        [RelayCommand]
        private async Task RefreshPageAsync()
        {
            try
            {
                await LoadUserAsync();
            }
            catch (Exception)
            {
                await ShowToast("Не удалось обновить данные");
            }
            finally
            {
                // Выключаем спиннер в конце
                IsBusy = false;

                IsRefreshing = false; 
            }
        }
    }
}