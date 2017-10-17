using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sino.Extensions.Apollo.Utils
{
    public class Properties
    {
        private Dictionary<string, string> _dict = new Dictionary<string, string>();

        public Properties(IDictionary<string,string> dictionary)
        {
            if(dictionary != null)
            {
                _dict = new Dictionary<string, string>(dictionary);
            }
        }

        public Properties(Properties source)
        {
            if(source != null && source._dict != null)
            {
                _dict = new Dictionary<string, string>(source._dict);
            }
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public string GetProperty(string key)
        {
            return GetProperty(key, null);
        }

        public string GetProperty(string key, string defaultValue)
        {
            string result = null;
            if(_dict.TryGetValue(key, out result))
            {
                return result;
            }
            return defaultValue;
        }

        public void SetProperty(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            _dict[key] = value;
        }

        public ICollection<string> GetPropertyNames()
        {
            return _dict.Keys;
        }

        public void Load(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _dict = (Dictionary<string, string>)serializer.Deserialize(sr, typeof(Dictionary<string, string>));
                }
            }
        }

        public void Store(string filePath)
        {
            using (var fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(sw, _dict);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if(obj == null || !(obj is Properties))
            {
                return false;
            }

            var source = _dict;
            var target = ((Properties)obj)._dict;

            if (target == null)
                return null == source;
            if (source == null)
                return false;
            if (object.ReferenceEquals(source, target))
                return true;
            if (source.Count != target.Count)
                return false;

            foreach(string k in source.Keys)
            {
                if (!target.ContainsKey(k))
                    return false;
                if (!source[k].Equals(target[k]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _dict.GetHashCode();
        }
    }
}
