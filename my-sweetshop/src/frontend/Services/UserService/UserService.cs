using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api.ProfileService;

namespace my_sweetshop.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IProfileService _api;

        public UserService(IProfileService api)
        {
            _api = api;
        }

        // Запрос на получения данных пользователя
        public Task<UserModel?> GetCurrentUser()
        {
            return _api.GetProfileAsync(forceRefresh: true);
        }

        // Запрос на смену имени и/или фамилии
        public Task UpdateProfileAsync(UpdateProfileDto dto)
        {
            return _api.UpdateProfileAsync(dto);
        }

        // Запрос на смену почты
        public async Task ChangeEmailAsync(string newEmail)
        {
            await _api.RequestChangeEmailAsync(newEmail);
        }
    }
}
