using my_sweetshop.Models;

namespace my_sweetshop.Dtos
{
    public class AuthVerifyCodeResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; }
        public bool NeedsProfile { get; set; }
        public UserModel User { get; set; } = null!;
    }
}
