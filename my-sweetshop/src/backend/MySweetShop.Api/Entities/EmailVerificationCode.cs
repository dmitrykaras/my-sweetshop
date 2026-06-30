namespace MySweetShop.Api.Entities;

public class EmailVerificationCode
{
    // ID пользователя
    public Guid Id { get; set; }
    // Почта
    public string Email { get; set; } = default!;
    // Код
    public string CodeHash { get; set; } = default!;
    // Время и дата создания
    public DateTimeOffset CreatedAt { get; set; }
    // Время конца действительности кода
    public DateTimeOffset ExpiresAt { get; set; }
    // Кол-во попыток ввода кода
    public int Attempts { get; set; }
    // Заморозка до определённого времени
    public DateTimeOffset? BlockedUntil { get; set; } = null;
    // Был ли код уже использован
    public bool IsUsed { get; set; }
}