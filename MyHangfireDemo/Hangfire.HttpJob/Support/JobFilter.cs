

using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using log4net;
using System;

namespace Hangfire.HttpJob.Support
{
    /// <summary>
    /// 任务过滤
    /// </summary>
    public class JobFilter : JobFilterAttribute, IClientFilter, IServerFilter,
        IElectStateFilter, IApplyStateFilter
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(JobFilter));

        //超时时间
        /// <summary>
        /// 分布式锁过期时间
        /// </summary>
        private readonly int _timeoutInSeconds;

        public JobFilter(int timeoutInSeconds)
        {
            if (timeoutInSeconds < 0)
                throw new ArgumentException("超时参数不能设置小于0的数");
            _timeoutInSeconds = timeoutInSeconds;
        }
        public void OnCreated(CreatedContext filterContext)
        {
            
            throw new NotImplementedException();
        }

        public void OnCreating(CreatingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public void OnStateElection(ElectStateContext context)
        {
            throw new NotImplementedException();
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            throw new NotImplementedException();
        }

    }
}
