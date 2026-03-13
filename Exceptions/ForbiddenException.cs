namespace Ecommerce_API.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message)
            : base(StatusCodes.Status403Forbidden, "FORBIDDEN", message )
        {
        }
    }
}
