using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message)
            : base((int)HttpStatusCode.Unauthorized, "UNAUTHORIZED", message)
        {

        }

    }
}
