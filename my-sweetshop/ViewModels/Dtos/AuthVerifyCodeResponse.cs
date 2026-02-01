

namespace my_sweetshop.ViewModels.Dtos
{
    public class AuthVerifyCodeResponse
    {
        public string Token { get; set; } = null!;
        public bool NeedsProfile { get; set; }

        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Points { get; set; }
    }

}
