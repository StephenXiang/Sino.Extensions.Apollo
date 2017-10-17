namespace Sino.Extensions.Apollo.Spi
{
    public interface IConfigFactoryManager
    {
        /// <summary>
        /// 根据指定命名空间获取配置工厂
        /// </summary>
        IConfigFactory GetFactory(string namespaceName);
    }
}
