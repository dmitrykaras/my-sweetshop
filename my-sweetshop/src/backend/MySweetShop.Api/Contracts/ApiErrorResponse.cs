namespace MySweetShop.Api.Contracts
{
    public class ApiErrorResponse
    {
        public string Error { get; set; } = default!;
        public string Message { get; set; } = default!;
        // Кол-во оставшихся попыток
        public int? AttemptsLeft { get; set; }
        // Время до следующей попытки
        public int? RetryAfterSeconds { get; set; }
    }
}