using NLog;
using Sino.Extensions.Apollo.Model;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sino.Extensions.Apollo.Utils;
using System.Linq;

namespace Sino.Extensions.Apollo.Core
{
    public abstract class AbstractConfig : IConfig
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public event ConfigChangeEvent ConfigChanged;

        public abstract string GetProperty(string key, string defaultValue);

        public int? GetIntProperty(string key, int? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : int.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetIntProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public long? GetLongProperty(string key, long? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : long.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetLongProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public short? GetShortProperty(string key, short? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : short.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetShortProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public float? GetFloatProperty(string key, float? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : float.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetFloatProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public double? GetDoubleProperty(string key, double? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : double.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetDoubleProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public sbyte? GetByteProperty(string key, sbyte? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : sbyte.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetByteProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public bool? GetBooleanProperty(string key, bool? defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                return value == null ? defaultValue : bool.Parse(value);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetBooleanProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public string[] GetArrayProperty(string key, string delimiter, string[] defaultValue)
        {
            try
            {
                string value = GetProperty(key, null);
                if (value == null)
                {
                    return defaultValue;
                }

                return Regex.Split(value, delimiter);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"GetArrayProperty for {key} failed, return default value {defaultValue}");
                return defaultValue;
            }
        }

        public abstract ICollection<string> GetPropertyNames();

        protected void FireConfigChange(ConfigChangeEventArgs changeEvent)
        {
            if(ConfigChanged != null)
            {
                foreach(ConfigChangeEvent handler in ConfigChanged.GetInvocationList())
                {
                    Task.Factory.StartNew(x =>
                    {
                        var h = (ConfigChangeEvent)x;
                        string methodName;
                        if(h.Target == null)
                        {
                            methodName = h.GetType().Name;
                        }
                        else
                        {
                            methodName = $"{h.Target.GetType()}.{h.GetType().Name}";
                        }
                        try
                        {
                            h(this, changeEvent);
                        }
                        catch(Exception ex)
                        {
                            logger.Error(ex, $"Failed to invoke config change handler {methodName}");
                        }
                    }, handler);
                }
            }
        }

        protected ICollection<ConfigChange> CalcPropertyChanges(string namespaceName, Properties previous, Properties current)
        {
            if(previous == null)
            {
                previous = new Properties();
            }

            if(current == null)
            {
                current = new Properties();
            }

            var previousKeys = previous.GetPropertyNames();
            var currentKeys = current.GetPropertyNames();

            var commonKeys = previousKeys.Intersect(currentKeys);
            var newKeys = currentKeys.Except(commonKeys);
            var removedKeys = previousKeys.Except(commonKeys);

            ICollection<ConfigChange> changes = new LinkedList<ConfigChange>();

            foreach(string newKey in newKeys)
            {
                changes.Add(new ConfigChange(namespaceName, newKey, null, current.GetProperty(newKey), Enums.PropertyChangeType.Added));
            }

            foreach(string removedKey in removedKeys)
            {
                changes.Add(new ConfigChange(namespaceName, removedKey, previous.GetProperty(removedKey), null, Enums.PropertyChangeType.Deleted));
            }

            foreach(string commonKey in commonKeys)
            {
                string previousValue = previous.GetProperty(commonKey);
                string currentValue = current.GetProperty(commonKey);
                if(string.Equals(previousValue, currentValue))
                {
                    continue;
                }
                changes.Add(new ConfigChange(namespaceName, commonKey, previousValue, currentValue, Enums.PropertyChangeType.Modified));
            }

            return changes;
        }
    }
}
