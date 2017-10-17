using Sino.Extensions.Apollo.Spi;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Sino.Extensions.Apollo.Core;

namespace Sino.Extensions.Apollo
{
    public class ConfigManager : IConfigManager
    {
        private IConfigFactoryManager _factoryManager;
        private IDictionary<string, IConfig> _configs = new ConcurrentDictionary<string, IConfig>();

        public IConfig GetConfig(string namespaceName)
        {
            IConfig config;
            _configs.TryGetValue(namespaceName, out config);

            if(config == null)
            {
                lock(this)
                {
                    _configs.TryGetValue(namespaceName, out config);

                    if(config == null)
                    {
                        IConfigFactory factory = _factoryManager.GetFactory(namespaceName);

                        config = factory.Create(namespaceName);
                        _configs[namespaceName] = config;
                    }
                }
            }

            return config;
        }

        public IConfig GetConfig()
        {
            return GetConfig(ConfigConsts.NAMESPACE_APPLICATION);
        }
    }
}
