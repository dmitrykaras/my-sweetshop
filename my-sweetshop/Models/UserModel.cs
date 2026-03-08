namespace my_sweetshop.Models
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Points { get; set; }
    }
}
