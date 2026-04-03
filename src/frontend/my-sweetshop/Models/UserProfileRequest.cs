using System.ComponentModel.DataAnnotations;

namespace my_sweetshop.Models;

public class UserProfileRequest
{
    public string Email { get; set; } = default!;
    [Required, MinLength(2)]
    public string FirstName { get; set; } = default!;
    [Required, MinLength(2)]
    public string LastName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}