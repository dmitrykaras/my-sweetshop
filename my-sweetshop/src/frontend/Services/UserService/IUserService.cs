using my_sweetshop.Models;
using my_sweetshop.Dtos;

namespace my_sweetshop.Services.UserService
{
    public interface IUserService
    {
        Task<UserModel> GetCurrentUser();
        Task UpdateProfileAsync(UpdateProfileDto dto);
        Task ChangeEmailAsync(string newEmail);
    }
}