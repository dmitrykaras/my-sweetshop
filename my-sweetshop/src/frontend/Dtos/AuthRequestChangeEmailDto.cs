namespace my_sweetshop.Dtos
{
    public class AuthRequestChangeEmailDto
    {
        public string NewEmail { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}