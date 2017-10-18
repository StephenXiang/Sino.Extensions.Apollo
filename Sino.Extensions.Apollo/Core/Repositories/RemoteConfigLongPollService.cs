using NLog;
using Sino.Extensions.Apollo.Configuration;
using Sino.Extensions.Apollo.Core.Dto;
using Sino.Extensions.Apollo.Core.Schedule;
using Sino.Extensions.Apollo.Utils;
using Sino.Extensions.Apollo.Utils.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using Sino.Extensions.Apollo.Exceptions;
using Newtonsoft.Json;
using System.Net;
using Sino.Extensions.Apollo.Enums;

namespace Sino.Extensions.Apollo.Core.Repositories
{
    public class RemoteConfigLongPollService
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private static readonly long INIT_NOTIFICATION_ID = -1;

        private ConfigServiceLocator _serviceLocator;
        private ApolloConfiguration _config;
        private HttpUtil _httpUtil;

        private ThreadSafe.Boolean _longPollingStarted;
        private ThreadSafe.Boolean _longPollingStopped;
        private ISchedulePolicy _longPollFailSchedulePolicyInSecond;
        private ISchedulePolicy _longPollSuccessSchedulePolicyInMS;
        private readonly IDictionary<string, ISet<RemoteConfigRepository>> _longPollNamespace;
        private readonly IDictionary<string, long?> _notifications;
        private readonly IDictionary<string, ApolloNotificationMessages> _remoteNotificationMessages;

        public RemoteConfigLongPollService(ConfigServiceLocator serviceLocator, ApolloConfiguration config, HttpUtil httpUtil)
        {
            _serviceLocator = serviceLocator;
            _config = config;
            _httpUtil = httpUtil;

            _longPollFailSchedulePolicyInSecond = new ExponentialSchedulePolicy(1, 120);
            _longPollSuccessSchedulePolicyInMS = new ExponentialSchedulePolicy(100, 1000);
            _longPollingStarted = new ThreadSafe.Boolean(false);
            _longPollingStopped = new ThreadSafe.Boolean(false);
            _longPollNamespace = new ConcurrentDictionary<string, ISet<RemoteConfigRepository>>();
            _notifications = new ConcurrentDictionary<string, long?>();
            _remoteNotificationMessages = new ConcurrentDictionary<string, ApolloNotificationMessages>();
        }

        public bool Submit(string namespaceName, RemoteConfigRepository remoteConfigRepository)
        {
            ISet<RemoteConfigRepository> remoteConfigRepositories = null;
            _longPollNamespace.TryGetValue(namespaceName, out remoteConfigRepositories);
            if(remoteConfigRepositories == null)
            {
                lock(this)
                {
                    _longPollNamespace.TryGetValue(namespaceName, out remoteConfigRepositories);
                    if(remoteConfigRepositories == null)
                    {
                        remoteConfigRepositories = new HashSet<RemoteConfigRepository>();
                        _longPollNamespace[namespaceName] = remoteConfigRepositories;
                    }
                }
            }
            bool added = remoteConfigRepositories.Add(remoteConfigRepository);
            if(!_notifications.ContainsKey(namespaceName))
            {
                lock(this)
                {
                    if(!_notifications.ContainsKey(namespaceName))
                    {
                        _notifications[namespaceName] = INIT_NOTIFICATION_ID;
                    }
                }
            }
            if(!_longPollingStarted.ReadFullFence())
            {
                StartLongPolling();
            }
            return added;
        }

        private void StartLongPolling()
        {
            if(!_longPollingStarted.CompareAndSet(false,true))
            {
                return;
            }
            try
            {
                string appId = _config.AppId;
                string cluster = _config.Cluster;
                string dataCenter = _config.DataCenter;

                Thread t = new Thread(() =>
                {
                    DoLongPollingRefresh(appId, cluster, dataCenter);
                });
                t.IsBackground = true;
                t.Start();
            }
            catch(Exception ex)
            {
                var exception = new ApolloConfigException("Schedule long polling refresh failed", ex);
                logger.Warn(exception);
            }
        }

        private void StopLongPollingRefresh()
        {
            _longPollingStopped.CompareAndSet(false, true);
        }

        private void DoLongPollingRefresh(string appId, string cluster, string dataCenter)
        {
            Random random = new Random();
            ServiceDTO lastServiceDto = null;

            while(!_longPollingStopped.ReadFullFence())
            {
                int sleepTime = 50;
                string url = null;
                try
                {
                    if(lastServiceDto == null)
                    {
                        IList<ServiceDTO> configServices = ConfigServices;
                        lastServiceDto = configServices[random.Next(configServices.Count)];
                    }

                    url = AssembleLongPollRefreshUrl(lastServiceDto.HomepageUrl, appId, cluster, dataCenter);

                    logger.Debug($"Long polling from {url}");
                    var request = new Utils.Http.HttpRequest(url);
                    request.Timeout = 600000;
                    var response = _httpUtil.DoGet<IList<ApolloConfigNotification>>(request).Result;

                    logger.Debug($"Long polling response: {response.StatusCode}, url:{url}");
                    if(response.StatusCode == HttpStatusCode.OK && response.Body != null)
                    {
                        UpdateNotifications(response.Body);
                        UpdateRemoteNotifications(response.Body);
                        Notify(lastServiceDto, response.Body);
                        _longPollSuccessSchedulePolicyInMS.Success();
                    }
                    else
                    {
                        sleepTime = _longPollSuccessSchedulePolicyInMS.Fail();
                    }

                    if (response.StatusCode == HttpStatusCode.NotModified && random.NextDouble() >= 0.5)
                        lastServiceDto = null;

                    _longPollFailSchedulePolicyInSecond.Success();
                }
                catch(Exception ex)
                {
                    lastServiceDto = null;
                    int sleepTimeInSecond = _longPollFailSchedulePolicyInSecond.Fail();
                    logger.Warn(ex, $"Long polling failed, will retry in {sleepTimeInSecond} seconds, appId:{appId}, cluster: {cluster}, namespace: {AssembleNamespaces()}, long polling url: {url}");
                    sleepTime = sleepTimeInSecond * 1000;
                }
                finally
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        private void Notify(ServiceDTO lastServiceDto, IList<ApolloConfigNotification> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return;

            foreach(ApolloConfigNotification notification in notifications)
            {
                string namespaceName = notification.NamespaceName;
                ISet<RemoteConfigRepository> registries = null;
                List<RemoteConfigRepository> toBeNotified = new List<RemoteConfigRepository>();
                _longPollNamespace.TryGetValue(namespaceName, out registries);
                ApolloNotificationMessages originalMessage = null;
                _remoteNotificationMessages.TryGetValue(namespaceName, out originalMessage);
                ApolloNotificationMessages remoteMessages = originalMessage == null ? null : originalMessage.Clone();

                if(registries != null && registries.Count > 0)
                {
                    toBeNotified.AddRange(registries);
                }

                ISet<RemoteConfigRepository> extraRegistries = null;
                _longPollNamespace.TryGetValue($"{namespaceName}.{ConfigFileFormat.Properties.GetString()}", out extraRegistries);
                if(extraRegistries != null && extraRegistries.Count > 0)
                {
                    toBeNotified.AddRange(extraRegistries);
                }
                foreach(RemoteConfigRepository remoteConfigRepository in toBeNotified)
                {
                    try
                    {
                        remoteConfigRepository.OnLongPollNotified(lastServiceDto, remoteMessages);
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(ex);
                    }
                }
            }
        }

        private void UpdateNotifications(IList<ApolloConfigNotification> deltaNotifications)
        {
            foreach(ApolloConfigNotification notification in deltaNotifications)
            {
                if (string.IsNullOrEmpty(notification.NamespaceName))
                    continue;
                string namespaceName = notification.NamespaceName;
                if(_notifications.ContainsKey(namespaceName))
                {
                    _notifications[namespaceName] = notification.NotificationId;
                }
                string namespaceNameWithPropertiesSuffix = $"{namespaceName}.{ConfigFileFormat.Properties.GetString()}";
                if(_notifications.ContainsKey(namespaceNameWithPropertiesSuffix))
                {
                    _notifications[namespaceNameWithPropertiesSuffix] = notification.NotificationId;
                }
            }
        }

        private void UpdateRemoteNotifications(IList<ApolloConfigNotification> deltaNotifications)
        {
            foreach(ApolloConfigNotification notification in deltaNotifications)
            {
                if (string.IsNullOrEmpty(notification.NamespaceName))
                    continue;
                if (notification.Messages == null || notification.Messages.IsEmpty())
                    continue;

                ApolloNotificationMessages localRemoteMessages = null;
                _remoteNotificationMessages.TryGetValue(notification.NamespaceName, out localRemoteMessages);
                if(localRemoteMessages == null)
                {
                    localRemoteMessages = new ApolloNotificationMessages();
                    _remoteNotificationMessages[notification.NamespaceName] = localRemoteMessages;
                }

                localRemoteMessages.MergeFrom(notification.Messages);
            }
        }

        private string AssembleNamespaces()
        {
            return string.Join(ConfigConsts.CLUSTER_NAMESPACE_SEPARATOR, _longPollNamespace.Keys);
        }

        private string AssembleLongPollRefreshUrl(string url, string appId, string cluster, string dataCenter)
        {
            StringBuilder sb = new StringBuilder();
            if(!url.EndsWith("/", StringComparison.Ordinal))
            {
                sb.Append(url + "/");
            }
            sb.Append("notifications/v2");
            sb.Append("?appId=" + WebUtility.UrlEncode(appId));
            sb.Append("&cluster=" + WebUtility.UrlEncode(cluster));
            sb.Append("&notification=" + WebUtility.UrlEncode(AssembleNotifications(_notifications)));
            if(!string.IsNullOrEmpty(dataCenter))
            {
                sb.Append("&dataCenter=" + WebUtility.UrlEncode(dataCenter));
            }
            string localIp = _config.LocalIp;
            if(!string.IsNullOrEmpty(localIp))
            {
                sb.Append("&ip=" + WebUtility.UrlEncode(localIp));
            }

            return sb.ToString();
        }

        private string AssembleNotifications(IDictionary<string, long?> notificationsMap)
        {
            IList<ApolloConfigNotification> notifications = new List<ApolloConfigNotification>();
            foreach(KeyValuePair<string,long?> kvp in notificationsMap)
            {
                ApolloConfigNotification notification = new ApolloConfigNotification();
                notification.NamespaceName = kvp.Key;
                notification.NotificationId = kvp.Value.GetValueOrDefault(INIT_NOTIFICATION_ID);
                notifications.Add(notification);
            }
            return JsonConvert.SerializeObject(notifications);
        }

        private IList<ServiceDTO> ConfigServices
        {
            get
            {
                IList<ServiceDTO> services = _serviceLocator.GetConfigServices();
                if(services.Count == 0)
                {
                    throw new ApolloConfigException("No available config service");
                }
                return services;
            }
        }
    }
}
