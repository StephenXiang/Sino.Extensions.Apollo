namespace Sino.Extensions.Apollo.Spi
{
    public interface IConfigRegistry
    {
        /// <summary>
        /// 注册指定命名空间的配置工厂
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        /// <param name="factory">配置工厂实例</param>
        void Register(string namespaceName, IConfigFactory factory);

        /// <summary>
        /// 根据指定命名空间获取注册的配置工厂
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        IConfigFactory GetFactory(string namespaceName);
    }
}
