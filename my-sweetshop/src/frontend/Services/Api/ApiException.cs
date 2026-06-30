using my_sweetshop.Dtos;

namespace my_sweetshop.Services.Api
{
    public class ApiException(int statusCode, string message, ApiErrorResponse? error = null) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
        public ApiErrorResponse? Error { get; } = error;
    }
}