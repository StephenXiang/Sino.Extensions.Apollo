namespace Sino.Extensions.Apollo.Utils.Http
{
    public class HttpRequest
    {
        public HttpRequest(string url)
        {
            Url = url;
            Timeout = 0;
            ReadTimeout = 0;
        }

        public string Url { get; private set; }

        public int Timeout { get; set; }

        public int ReadTimeout { get; set; }
    }
}
