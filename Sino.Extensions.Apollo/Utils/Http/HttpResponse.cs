using System.Net;

namespace Sino.Extensions.Apollo.Utils.Http
{
    public class HttpResponse<T>
    {
        public HttpResponse(HttpStatusCode statusCode, T body)
        {
            StatusCode = statusCode;
            Body = body;
        }

        public HttpResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Body = default(T);
        }

        public HttpStatusCode StatusCode { get; private set; }

        public T Body { get; private set; }
    }
}
