namespace my_sweetshop.ViewModels.Dtos
{
    public class AuthVerifyCodeRequest
    {
        public string Email { get; set; } = "";
        public string Code { get; set; } = "";
    }
}
