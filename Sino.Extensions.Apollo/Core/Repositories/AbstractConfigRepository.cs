using NLog;
using System;
using System.Collections.Generic;
using Sino.Extensions.Apollo.Utils;

namespace Sino.Extensions.Apollo.Core.Repositories
{
    public abstract class AbstractConfigRepository : IConfigRepository
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private IList<IRepositoryChangeListener> _listeners = new List<IRepositoryChangeListener>();

        protected bool TrySync()
        {
            try
            {
                Sync();
                return true;
            }
            catch(Exception ex)
            {
                logger.Warn(ex, $"Sync config failed, will retry. Repository {GetType()}");
            }
            return false;
        }

        protected abstract void Sync();

        public abstract Properties GetConfig();

        public abstract void SetUpstreamRepository(IConfigRepository upstreamConfigRepository);

        public void AddChangeListener(IRepositoryChangeListener listener)
        {
            if(!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void RemoveChangeListener(IRepositoryChangeListener listener)
        {
            _listeners.Remove(listener);
        }

        protected void FireRepositoryChange(string namespaceName, Properties newProperties)
        {
            foreach(IRepositoryChangeListener listener in _listeners)
            {
                try
                {
                    listener.OnRepositoryChange(namespaceName, newProperties);
                }
                catch(Exception ex)
                {
                    logger.Error(ex, $"Failed to invoke repository change listener {listener.GetType()}");
                }
            }
        }
    }
}
