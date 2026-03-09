using my_sweetshop.Dtos;
using my_sweetshop.Models;

namespace my_sweetshop.Services.Api.ProfileService
{
    public interface IProfileService
    {
        Task<UserModel> GetProfileAsync(bool forceRefresh = false);
        Task<bool> UpdateProfileAsync(UpdateProfileDto dto);
        Task RequestChangeEmailAsync(string newEmail);
        void ClearCache(); // Например, при выходе из аккаунта (Logout)
    }
}