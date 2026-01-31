namespace MyCandyShop.Api.Contracts;

public class AuthRequestChangeEmailRequest
{
    public string NewEmail { get; set; } = default!;
}