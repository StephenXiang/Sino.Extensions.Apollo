namespace Sino.Extensions.Apollo.Core.Dto
{
    public class ServiceDTO
    {
        public string AppName { get; set; }

        public string HomepageUrl { get; set; }

        public string InstanceId { get; set; }

        public override string ToString()
        {
            return $"ServiceDTO{{appName={AppName},instanceId={InstanceId},homepageUrl={HomepageUrl}}}";
        }
    }
}
