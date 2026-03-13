using Ecommerce_API.Middleware;

namespace Ecommerce_API.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; }

        public string ErrorCode { get; }

        protected BaseException(int statusCode, string message, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
