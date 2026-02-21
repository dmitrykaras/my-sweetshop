namespace my_sweetshop.Dtos
{
    public class ApiErrorResponse
    {
        public string Error { get; set; } = "";
        public string Message { get; set; } = "";

        public int? AttemptsLeft { get; set; }
        public int? RetryAfterSeconds { get; set; }
    }
}