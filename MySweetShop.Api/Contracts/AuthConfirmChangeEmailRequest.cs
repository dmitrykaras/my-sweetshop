namespace MySweetShop.Api.Contracts;

public class AuthConfirmChangeEmailRequest
{
    public string NewEmail { get; set; } = default!;
    public string Code { get; set; } = default!;
}