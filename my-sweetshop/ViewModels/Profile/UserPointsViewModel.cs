using my_sweetshop.Services.UserService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace my_sweetshop.ViewModels.Profile
{
    public class UserPointsViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private int _totalPoints;

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public ICommand RefreshPointsCommand { get; }

        public UserPointsViewModel(IUserService userService)
        {
            _userService = userService;
            RefreshPointsCommand = new Command(async () => await LoadPointsAsync());

            _ = LoadPointsAsync(); // Фоновая загрузка
        }

        // Метод загрузки кол-ва баллов текущего пользователя
        public async Task LoadPointsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var user = await _userService.GetCurrentUser();

                // Проверяем, удалось ли получить пользователя (например, он не вышел из аккаунта)
                if (user != null)
                {
                    TotalPoints = user.Points;
                }
                else
                {
                    // Если пользователя нет, можно сбросить баллы в 0
                    TotalPoints = 0;
                }
            }
            catch (Exception ex)
            {
                // Защита на случай других непредвиденных ошибок сети или парсинга
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке баллов: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}