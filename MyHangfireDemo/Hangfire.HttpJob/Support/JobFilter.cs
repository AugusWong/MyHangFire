

using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using log4net;
using log4net.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using UtilRepo;

namespace Hangfire.HttpJob.Support
{
    /// <summary>
    /// 任务过滤
    /// </summary>
    public class JobFilter : JobFilterAttribute, IClientFilter, IServerFilter,
        IElectStateFilter, IApplyStateFilter
    {
        // private static readonly ILog log = log4net.LogManager.GetCurrentClassLogger(typeof(JobFilter));
        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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

            log.InfoFormat("创建任务 `{0}` id为 `{1}`",
                filterContext.Job.Method.Name,
                filterContext.BackgroundJob?.Id);
        }

        public void OnCreating(CreatingContext filterContext)
        {
            log.Info($"开始创建任务:{filterContext.Job.Method.Name}");
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            if (!filterContext.Items.ContainsKey("DistributedLock"))
                throw new InvalidOperationException("找不到分布式锁，没有为该任务申请分布式锁");

            //释放分布式锁
            var distributedLock = (IDisposable)filterContext.Items["DistributedLock"];
            distributedLock.Dispose();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            DeleteLogFiles();
            //设置分布式锁，分布式锁会阻止两个相同的任务并发执行，用任务名称和方法名称作为锁
            var jobName = filterContext.BackgroundJob.Job.Args[1];
            var methodName = filterContext.BackgroundJob.Job.Method.Name;
            var jobResource = $"{jobName}.{methodName}";
            var locktimeOut = TimeSpan.FromSeconds(_timeoutInSeconds);

            try
            {
                //判断任务是否被暂停
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var conts = connection.GetAllItemsFromSet($"JobPauseOf:{jobName}");
                    if (conts.Contains("true"))
                    {
                        filterContext.Canceled = true;//任务被暂停不执行直接跳过
                        return;
                    }
                }
                //申请分布式锁
                var distributedLock = filterContext.Connection.AcquireDistributedJobLock(jobResource, locktimeOut);
                filterContext.Items["DistributedLock"] = distributedLock;
            }
            catch (Exception ex)
            {
                //获取锁超市，取消任务，任务会默认置为成功
                filterContext.Canceled = true;
                log.Error($"任务{jobName}超时,任务id{filterContext.BackgroundJob.Id},异常信息:{ex}");
            }
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            //设置过期时间，任务将在三天后过期，过期的任务会自动被扫描并删除
            context.JobExpirationTimeout = TimeSpan.FromDays(3);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState failedState)
            {
                log.Warn($"任务 `{context.BackgroundJob.Id}` 执行失败，异常为 `{failedState.Exception}`");
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(3);
        }

        /// <summary>
        /// 清除日志文件，每隔20天按日期清理一次
        /// </summary>
        /// <returns></returns>
        private Task DeleteLogFiles()
        {
            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}/logs/");
            if (!dir.Exists) return Task.CompletedTask;
            var taskdelete = Task.Run(() =>
            {
                try
                {
                    FileSystemInfo[] fileinfos = dir.GetFileSystemInfos();
                    foreach (var fi in fileinfos)
                    {
                        if (fi is DirectoryInfo)//判断是否是文件夹
                        {
                            var dirdate = fi.Name.ToDateTime();
                            if (DateTime.Now.Subtract(dirdate).TotalDays >= 20)
                            {
                                DirectoryInfo subdir = new DirectoryInfo(fi.FullName);
                                subdir.Delete(true); //删除子目录和文件
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("删除日志文件出错:", ex);
                }
            });
            return Task.CompletedTask;
        }

    }
}
