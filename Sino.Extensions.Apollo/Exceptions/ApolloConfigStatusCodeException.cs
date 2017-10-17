using System;
using System.Net;

namespace Sino.Extensions.Apollo.Exceptions
{
    public class ApolloConfigStatusCodeException : Exception
    {
        public virtual HttpStatusCode StatusCode { get; private set; }

        public ApolloConfigStatusCodeException(HttpStatusCode statusCode, string message)
            :base($"[status code:{statusCode},{message}]")
        {
            StatusCode = statusCode;
        }
    }
}
