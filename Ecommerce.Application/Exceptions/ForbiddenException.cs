using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message)
            : base((int)HttpStatusCode.Forbidden, "FORBIDDEN", message)
        {
        }
    }
}
