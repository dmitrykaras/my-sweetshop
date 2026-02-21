using my_sweetshop.Dtos;
using my_sweetshop.Models;
using my_sweetshop.Services.Api;

namespace my_sweetshop.Services.Domain
{
    public class UserService : IUserService
    {
        private readonly ApiService _api;

        public UserService(ApiService api)
        {
            _api = api;
        }

        public Task<UserModel> GetCurrentUser()
        {
            return _api.GetUserAsync();
        }

        public Task UpdateProfileAsync(UpdateProfileDto dto)
        {
            return _api.UpdateProfileAsync(dto.FirstName, dto.LastName);
        }

        public async Task ChangeEmailAsync(string newEmail)
        {
            await _api.RequestChangeEmailAsync(newEmail);
        }
    }
}
