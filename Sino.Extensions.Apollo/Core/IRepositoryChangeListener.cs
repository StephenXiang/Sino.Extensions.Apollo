using Sino.Extensions.Apollo.Utils;

namespace Sino.Extensions.Apollo.Core
{
    public interface IRepositoryChangeListener
    {
        /// <summary>
        /// 当配置仓储发生改变时调用
        /// </summary>
        void OnRepositoryChange(string namespaceName, Properties newProperties);
    }
}
