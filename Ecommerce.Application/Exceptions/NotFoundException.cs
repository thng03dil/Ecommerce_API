using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
            : base((int)HttpStatusCode.NotFound, "NOT_FOUND", message)
        {
        }
    }
}
