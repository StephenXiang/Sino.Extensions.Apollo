using Newtonsoft.Json;
using Sino.Extensions.Apollo.Configuration;
using Sino.Extensions.Apollo.Exceptions;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sino.Extensions.Apollo.Utils.Http
{
    public class HttpUtil
    {
        public static HttpClient Http = new HttpClient();

        private ApolloConfiguration _config;
        private string _basicAuth;

        public HttpUtil(ApolloConfiguration config)
        {
            _config = config;
            _basicAuth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("user:"));
        }

        public async Task<HttpResponse<T>> DoGet<T>(HttpRequest httpRequest)
        {
            try
            {
                int timeout = httpRequest.Timeout;
                if (timeout <= 0 && timeout != Timeout.Infinite)
                {
                    timeout = _config.Timeout;
                }

                int readTimeout = httpRequest.ReadTimeout;
                if (readTimeout <= 0 && readTimeout != Timeout.Infinite)
                {
                    readTimeout = _config.ReadTimeout;
                }

                Http.DefaultRequestHeaders.Add("Authorization", _basicAuth);
                Http.Timeout = TimeSpan.FromMilliseconds(timeout);

                var response = await Http.GetAsync(httpRequest.Url);
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (response)
                    {
                        var stream = await response.Content.ReadAsStringAsync();
                        T body = JsonConvert.DeserializeObject<T>(stream);
                        return new HttpResponse<T>(response.StatusCode, body);
                    }
                }
                else if (response != null && response.StatusCode == HttpStatusCode.NotModified)
                {
                    return new HttpResponse<T>(response.StatusCode);
                }
                else
                {
                    throw new ApolloConfigStatusCodeException(response.StatusCode, $"Get operation failed for {httpRequest.Url}");
                }
            }
            catch (Exception ex)
            {
                throw new ApolloConfigException("Could not complete get operation", ex);
            }
        }
    }
}
