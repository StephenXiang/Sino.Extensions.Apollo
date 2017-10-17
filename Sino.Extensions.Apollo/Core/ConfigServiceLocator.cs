using NLog;
using Sino.Extensions.Apollo.Configuration;
using Sino.Extensions.Apollo.Core.Dto;
using Sino.Extensions.Apollo.Utils.Http;
using System;
using System.Collections.Generic;
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

        }
    }
}
