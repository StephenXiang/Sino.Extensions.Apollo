using System;
using System.Collections.Generic;

namespace Sino.Extensions.Apollo.Model
{
    public class ConfigChangeEventArgs : EventArgs
    {
        private readonly IDictionary<string, ConfigChange> _changes;

        public ConfigChangeEventArgs(string namespaceName, IDictionary<string, ConfigChange> changes)
        {
            Namespace = namespaceName;
            _changes = changes;
        }

        public ICollection<string> ChangedKeys
        {
            get
            {
                return _changes.Keys;
            }
        }

        public ConfigChange GetChange(string key)
        {
            ConfigChange change;
            _changes.TryGetValue(key, out change);
            return change;
        }

        public string Namespace { get; private set; }
    }
}
