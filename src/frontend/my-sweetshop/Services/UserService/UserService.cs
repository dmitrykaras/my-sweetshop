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

        public Task<UserModel> GetCurrentUser()
        {
            return _api.GetProfileAsync(forceRefresh: true);
        }

        public Task UpdateProfileAsync(UpdateProfileDto dto)
        {
            return _api.UpdateProfileAsync(dto);
        }

        public async Task ChangeEmailAsync(string newEmail)
        {
            await _api.RequestChangeEmailAsync(newEmail);
        }
    }
}
