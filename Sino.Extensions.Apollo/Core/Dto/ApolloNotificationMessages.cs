using System.Collections.Generic;

namespace Sino.Extensions.Apollo.Core.Dto
{
    public class ApolloNotificationMessages
    {
        public IDictionary<string, long> Details { get; set; }

        public ApolloNotificationMessages()
            : this(new Dictionary<string, long>()) { }

        public ApolloNotificationMessages(IDictionary<string, long> details)
        {
            Details = details;
        }

        public void Put(string key, long notificationId)
        {
            Details[key] = notificationId;
        }

        public long Get(string key)
        {
            return Details[key];
        }

        public bool Has(string key)
        {
            return Details.ContainsKey(key);
        }

        public bool IsEmpty()
        {
            return Details.Count == 0;
        }

        public void MergeFrom(ApolloNotificationMessages source)
        {
            if (source == null)
            {
                return;
            }

            foreach (KeyValuePair<string, long> entry in source.Details)
            {
                if (Has(entry.Key) && Get(entry.Key) >= entry.Value)
                {
                    continue;
                }
                Put(entry.Key, entry.Value);
            }
        }

        public ApolloNotificationMessages Clone()
        {
            return new ApolloNotificationMessages(new Dictionary<string, long>(Details));
        }
    }
}
