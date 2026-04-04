using my_sweetshop.Dtos;

namespace my_sweetshop.Services.Api
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public ApiErrorResponse? Error { get; }

        public ApiException(int statusCode, string message, ApiErrorResponse? error = null)
            : base(message)
        {
            StatusCode = statusCode;
            Error = error;
        }
    }
}