namespace MySweetShop.Api.Contracts;

public class AuthVerifyCodeRequest
{
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";
}