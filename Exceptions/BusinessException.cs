using System.Net;

namespace Ecommerce_API.Exceptions
{
    public class BusinessException : BaseException
    {
        public BusinessException( string message)
            : base(StatusCodes.Status400BadRequest, "BUSINESS_ERROR", message)
        {

        }
    }
}
