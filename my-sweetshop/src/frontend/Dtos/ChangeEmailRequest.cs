namespace my_sweetshop.Dtos
{
    public class ChangeEmailRequest
    {
        public required string NewEmail { get; set; }
        public required string Code { get; set; }
    }
}
