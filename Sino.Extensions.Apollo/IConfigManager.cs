namespace Sino.Extensions.Apollo
{
    public interface IConfigManager
    {
        /// <summary>
        /// 根据指定的命名空间获取配置实例
        /// </summary>
        /// <param name="namespaceName">命名空间</param>
        IConfig GetConfig(string namespaceName);

        /// <summary>
        /// 获取默认命名空间的配置实例
        /// </summary>
        IConfig GetConfig();
    }
}
