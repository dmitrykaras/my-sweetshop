namespace my_sweetshop.Dtos
{
    public class AuthVerifyCodeRequest
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
    }
}
