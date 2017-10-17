namespace Sino.Extensions.Apollo.Spi
{
    public interface IConfigFactory
    {
        /// <summary>
        /// 根据指定的命名空间获取配置实例
        /// </summary>
        IConfig Create(string namespaceName);
    }
}
