using Sino.Extensions.Apollo.Enums;

namespace Sino.Extensions.Apollo.Model
{
    public class ConfigChange
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="oldValue">修改前的值</param>
        /// <param name="newValue">修改后的值</param>
        /// <param name="changeType">变动类型</param>
        public ConfigChange(string namespaceName, string propertyName, string oldValue, string newValue,
            PropertyChangeType changeType)
        {
            Namespace = namespaceName;
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
            ChangeType = changeType;
        }

        public string PropertyName { get; private set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        public PropertyChangeType ChangeType { get; set; }

        public string Namespace { get; private set; }

        public override string ToString()
        {
            return $"ConfigChange[namespace={Namespace}, propertyName={PropertyName}, oldValue={OldValue}, newValue={NewValue}, changeType={ChangeType}]";
        }
    }
}
