using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Sino.Extensions.Apollo.Spi
{
    public class ConfigFactoryManager : IConfigFactoryManager
    {
        private IConfigRegistry _registry;
        private IDictionary<string, IConfigFactory> _factories = new ConcurrentDictionary<string, IConfigFactory>();

        public IConfigFactory GetFactory(string namespaceName)
        {
            IConfigFactory factory = _registry.GetFactory(namespaceName);

            if(factory != null)
            {
                return factory;
            }

            if(_factories.TryGetValue(namespaceName, out factory))
            {
                return factory;
            }

            _factories[namespaceName] = factory;

            return factory;
        }
    }
}
