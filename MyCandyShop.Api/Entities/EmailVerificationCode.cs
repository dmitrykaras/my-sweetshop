namespace MyCandyShop.Api.Entities;

public class EmailVerificationCode
{
    public Guid Id { get; set; }

    public string Email { get; set; } = default!;
    public string CodeHash { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public int Attempts { get; set; }
    public bool IsUsed { get; set; }
}