using Microsoft.AspNetCore.Http;
using System.Net;

namespace Ecommerce_API.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException( string message) 
            : base(StatusCodes.Status404NotFound, "NOT_FOUND",message)  
        { 
        }
    }
}
