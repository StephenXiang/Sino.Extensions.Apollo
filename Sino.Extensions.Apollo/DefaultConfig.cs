using NLog;
using Sino.Extensions.Apollo.Core;
using Sino.Extensions.Apollo.Enums;
using Sino.Extensions.Apollo.Model;
using Sino.Extensions.Apollo.Utils;
using System;
using System.Collections.Generic;

namespace Sino.Extensions.Apollo
{
    public class DefaultConfig : AbstractConfig, IRepositoryChangeListener
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly string _namespace;
        private volatile Properties _configProperties;
        private IConfigRepository _configRepository;

        public DefaultConfig(string namespaceName, IConfigRepository configRepository)
        {
            _namespace = namespaceName;
            _configRepository = configRepository;
            _configProperties = new Properties();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _configProperties = _configRepository.GetConfig();
            }
            catch(Exception ex)
            {
                logger.Warn(ex, $"Init Apollo Local Config failed - namespace : {_namespace}");
            }
            finally
            {
                _configRepository.AddChangeListener(this);
            }
        }

        public override string GetProperty(string key, string defaultValue)
        {
            string value = null;

            if(_configProperties != null)
            {
                value = _configProperties.GetProperty(key);
            }

            if(value == null)
            {
                value = Environment.GetEnvironmentVariable(key);
            }

            if(value == null && _configProperties == null)
            {
                logger.Warn($"Could not load config for namespace {_namespace} from Apollo, please check whether the configs are released in Apollo! Return default value now!");
            }

            return value == null ? defaultValue : value;
        }

        public void OnRepositoryChange(string namespaceName, Properties newProperties)
        {
            lock (this)
            {
                Properties newConfigProperties = new Properties(newProperties);

                IDictionary<string, ConfigChange> actualChanges = UpdateAndCalcConfigChange(newConfigProperties);

                if (actualChanges.Count == 0)
                    return;

                FireConfigChange(new ConfigChangeEventArgs(_namespace, actualChanges));
            }
        }

        private IDictionary<string, ConfigChange> UpdateAndCalcConfigChange(Properties newConfigProperties)
        {
            ICollection<ConfigChange> configChanges = CalcPropertyChanges(_namespace, _configProperties, newConfigProperties);
            IDictionary<string, ConfigChange> actualChanges = new Dictionary<string, ConfigChange>();

            foreach(ConfigChange change in configChanges)
            {
                change.OldValue = GetProperty(change.PropertyName, change.OldValue);
            }

            _configProperties = newConfigProperties;

            foreach(ConfigChange change in configChanges)
            {
                change.NewValue = GetProperty(change.PropertyName, change.NewValue);
                switch(change.ChangeType)
                {
                    case PropertyChangeType.Added:
                        {
                            if (string.Equals(change.OldValue, change.NewValue))
                                break;
                            if (change.OldValue != null)
                                change.ChangeType = PropertyChangeType.Modified;
                            actualChanges[change.PropertyName] = change;
                        }
                        break;
                    case PropertyChangeType.Modified:
                        {
                            if (!string.Equals(change.OldValue, change.NewValue))
                                actualChanges[change.PropertyName] = change;
                        }
                        break;
                    case PropertyChangeType.Deleted:
                        {
                            if (string.Equals(change.OldValue, change.NewValue))
                                break;
                            if (change.NewValue != null)
                                change.ChangeType = PropertyChangeType.Modified;
                            actualChanges[change.PropertyName] = change;
                        }
                        break;
                    default:
                        break;
                }
            }
            return actualChanges;
        }

        public override ICollection<string> GetPropertyNames()
        {
            if (_configProperties == null)
                return new List<string>();

            return _configProperties.GetPropertyNames();
        }
    }
}
