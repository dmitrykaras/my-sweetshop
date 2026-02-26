namespace my_sweetshop.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = default!;
    }
}
