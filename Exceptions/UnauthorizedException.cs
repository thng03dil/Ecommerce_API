using System.Net;

namespace Ecommerce_API.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message)
            : base((int)HttpStatusCode.Unauthorized, "UNAUTHORIZED", message)
        {

        }

    }
}