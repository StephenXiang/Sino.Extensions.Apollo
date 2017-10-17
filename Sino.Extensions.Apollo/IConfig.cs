using Sino.Extensions.Apollo.Model;
using System.Collections.Generic;

namespace Sino.Extensions.Apollo
{
    public delegate void ConfigChangeEvent(object sender, ConfigChangeEventArgs args);

    public interface IConfig
    {
        /// <summary>
        /// 根据键获取值，如果键不存在则返回 defaultValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetProperty(string key, string defaultValue);

        int? GetIntProperty(string key, int? defaultValue);

        long? GetLongProperty(string key, long? defaultValue);

        short? GetShortProperty(string key, short? defaultValue);

        float? GetFloatProperty(string key, float? defaultValue);

        double? GetDoubleProperty(string key, double? defaultValue);

        sbyte? GetByteProperty(string key, sbyte? defaultValue);

        bool? GetBooleanProperty(string key, bool? defaultValue);

        string[] GetArrayProperty(string key, string delimiter, string[] defaultValue);

        ICollection<string> GetPropertyNames();

        event ConfigChangeEvent ConfigChanged;
    }
}
