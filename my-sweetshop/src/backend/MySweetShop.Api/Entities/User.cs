namespace MySweetShop.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Points { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; }
    // Последнее изменение профиля (время и дата)
    public DateTimeOffset? LastProfileUpdate { get; set; }
}