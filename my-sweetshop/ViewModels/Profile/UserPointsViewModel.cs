using my_sweetshop.Services.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

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
                TotalPoints = user.Points;
            }
            finally { IsBusy = false; }
        }
    }
}
