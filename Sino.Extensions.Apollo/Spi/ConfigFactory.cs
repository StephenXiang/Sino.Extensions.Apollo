using NLog;
using Sino.Extensions.Apollo.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Extensions.Apollo.Spi
{
    public class ConfigFactory : IConfigFactory
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private ApolloConfiguration _config;

        public ConfigFactory(ApolloConfiguration config)
        {
            _config = config;
        }

        public IConfig Create(string namespaceName)
        {
            var defaultConfig = new DefaultConfig(namespaceName, CreateLocalConfigRepository(namespaceName));
            return defaultConfig;
        }


    }
}
