using Sino.Extensions.Apollo.Utils;

namespace Sino.Extensions.Apollo.Core.Repositories
{
    public interface IConfigRepository
    {
        /// <summary>
        /// 从该仓储获取配置数据
        /// </summary>
        Properties GetConfig();

        /// <summary>
        /// 修改该仓储的数据来源
        /// </summary>
        void SetUpstreamRepository(IConfigRepository upstreamConfigRepository);

        /// <summary>
        /// 添加监听
        /// </summary>
        void AddChangeListener(IRepositoryChangeListener listener);

        /// <summary>
        /// 移除监听
        /// </summary>
        void RemoveChangeListener(IRepositoryChangeListener listener);
    }
}
