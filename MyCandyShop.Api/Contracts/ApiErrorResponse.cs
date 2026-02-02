namespace MyCandyShop.Api.Contracts
{
    public class ApiErrorResponse
    {
        public string Error { get; set; } = default!;
        public string Message { get; set; } = default!;
        public int? AttemptsLeft { get; set; }
        public int? RetryAfterSeconds { get; set; }
    }
}