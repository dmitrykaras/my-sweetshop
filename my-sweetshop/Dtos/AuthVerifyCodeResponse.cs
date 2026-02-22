namespace my_sweetshop.Dtos
{
    public class AuthVerifyCodeResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; }
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
