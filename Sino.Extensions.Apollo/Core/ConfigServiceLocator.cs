using NLog;
using Sino.Extensions.Apollo.Configuration;
using Sino.Extensions.Apollo.Core.Dto;
using Sino.Extensions.Apollo.Exceptions;
using Sino.Extensions.Apollo.Utils.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Sino.Extensions.Apollo.Core
{
    public class ConfigServiceLocator
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private HttpUtil _httpUtil;
        private ApolloConfiguration _config;
        private volatile IList<ServiceDTO> _configServices;

        public ConfigServiceLocator(HttpUtil httpUtil, ApolloConfiguration config)
        {
            _httpUtil = httpUtil;
            _config = config;
            _configServices = new List<ServiceDTO>();
        }

        public void Initialize()
        {
            TryUpdateConfigServices();
            SchedulePeriodicRefresh();
        }

        public IList<ServiceDTO> GetConfigServices()
        {
            if(_configServices.Count == 0)
            {
                UpdateConfigServices();
            }

            return _configServices;
        }

        private bool TryUpdateConfigServices()
        {
            try
            {
                UpdateConfigServices();
                return true;
            }
            catch (Exception) { }

            return false;
        }

        private void SchedulePeriodicRefresh()
        {
            Thread t = new Thread(() =>
            {
                while(true)
                {
                    try
                    {
                        Thread.Sleep(_config.RefreshInterval);
                        logger.Debug("refresh config services");
                        TryUpdateConfigServices();
                    }
                    catch (Exception) { }
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private void UpdateConfigServices()
        {
            lock(this)
            {
                string url = AssembleMetaServiceUrl();

                var requset = new Sino.Extensions.Apollo.Utils.Http.HttpRequest(url);
                int maxRetries = 5;
                Exception exception = null;
                for(int i = 0;i < maxRetries; i++)
                {
                    try
                    {
                        var response = _httpUtil.DoGet<IList<ServiceDTO>>(requset).Result;
                        var services = response.Body;
                        if (services == null || services.Count == 0)
                            continue;
                        _configServices = services;
                        return;
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(ex);
                        exception = ex;
                    }
                    Thread.Sleep(1000);
                }

                throw new ApolloConfigException($"Get config services failed from {url}", exception);
            }
        }

        private string AssembleMetaServiceUrl()
        {
            string domainName = _config.MetaServerDomainName;
            string appId = _config.AppId;
            string localIp = _config.LocalIp;
            StringBuilder sb = new StringBuilder();
            sb.Append(domainName + "/services/config");
            sb.Append("?appId=" + WebUtility.UrlEncode(appId));
            if(!string.IsNullOrEmpty(localIp))
            {
                sb.Append("&ip=" + WebUtility.UrlEncode(localIp));
            }

            return sb.ToString();
        }
    }
}
