namespace MyCandyShop.Api.Contracts;

public class AuthRequestChangeEmailRequest
{
    public string Email { get; set; } = default!;
    public string NewEmail { get; set; } = default!;
}