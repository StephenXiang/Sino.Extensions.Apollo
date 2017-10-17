namespace Sino.Extensions.Apollo.Configuration
{
    public class ApolloConfiguration
    {
        public string AppId { get; set; }

        public string HostAddress { get; set; }

        public string HostName { get; set; }

        public string EnvType { get; set; }

        public string SubEnvType { get; set; }

        public string DataCenter { get; set; }

        public string Cluster { get; set; }

        public int Timeout { get; set; }

        public int ReadTimeout { get; set; }

        public int RefreshInterval { get; set; }

        public string MetaServerDomainName { get; set; }
    }
}
