using System;
using System.Collections.Generic;
using System.Text;

namespace Sino.Extensions.Apollo.Model
{
    public class ConfigChangeEventArgs : EventArgs
    {
        private readonly string _namespace;
        private readonly IDictionary<string, ConfigChange> _changes;

        public ConfigChangeEventArgs(string namespaceName, IDictionary<string, ConfigChange> changes)
        {
            _namespace = namespaceName;
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


    }
}
