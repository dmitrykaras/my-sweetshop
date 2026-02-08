namespace MyCandyShop.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public int Points { get; set; } = 0;

    public DateTimeOffset CreatedAt { get; set; }
}