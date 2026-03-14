using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Ecommerce.Application.Exceptions
{
    public class BusinessException : BaseException
    {
        public BusinessException(string message)
            : base((int)HttpStatusCode.BadRequest, "BUSINESS_ERROR", message)
        {

        }
    }
}
