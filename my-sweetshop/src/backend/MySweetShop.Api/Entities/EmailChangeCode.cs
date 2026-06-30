namespace MySweetShop.Api.Entities;

public class EmailChangeCode
{
    // ID запроса
    public Guid Id { get; set; }
    // ID пользователя
    public Guid UserId { get; set; }
    // Почта
    public string Email { get; set; } = default!;
    // Новая почта
    public string NewEmail { get; set; } = default!;
    // Хешкод
    public string CodeHash { get; set; } = default!;
    // Время и дата созданя
    public DateTimeOffset CreatedAt { get; set; }
    // Время конца действительности кода
    public DateTimeOffset ExpiresAt { get; set; }
    // Кол-во попыток ввода кода
    public int Attempts { get; set; }
    // Был ли код уже использован
    public bool IsUsed { get; set; }
}