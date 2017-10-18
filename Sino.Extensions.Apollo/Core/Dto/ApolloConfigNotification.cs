namespace Sino.Extensions.Apollo.Core.Dto
{
    public class ApolloConfigNotification
    {
        public string NamespaceName { get; set; }

        public long NotificationId { get; set; }

        public ApolloNotificationMessages Messages { get; set; }

        public ApolloConfigNotification() { }

        public ApolloConfigNotification(string namespaceName, long notificationId)
        {
            NamespaceName = namespaceName;
            NotificationId = notificationId;
        }

        public void AddMessage(string key, long notificationId)
        {
            if(Messages == null)
            {
                lock(this)
                {
                    if(Messages == null)
                    {
                        Messages = new ApolloNotificationMessages();
                    }
                }
            }
            Messages.Put(key, notificationId);
        }

        public override string ToString()
        {
            return $"ApolloConfigNotification{{namespaceName={NamespaceName},notificationId={NotificationId}}}";
        }
    }
}
