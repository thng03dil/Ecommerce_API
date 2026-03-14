using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Exceptions
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
