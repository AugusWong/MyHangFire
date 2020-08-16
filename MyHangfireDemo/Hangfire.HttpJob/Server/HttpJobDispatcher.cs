using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.Logging;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UtilRepo;

namespace Hangfire.HttpJob.Server
{
    public class HttpJobDispatcher : IDashboardDispatcher
    {
        private static readonly ILog Logger = LogProvider.For<HttpJobDispatcher>();

        public HttpJobDispatcher(HangfireHttpJobOptions options)
        {
            if (options.IsNull())
                throw new ArgumentNullException(nameof(options));
        }
        public Task Dispatch([NotNull] DashboardContext context)
        {
            if (context.IsNull())
                throw new NotImplementedException(nameof(context));

            try
            {
                if (!"POST".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = HttpStatusCode.MethodNotAllowed.ToInt();
                    return Task.FromResult(false);
                }

                var op = context.Request.GetQuery("op");
                if (op.IsNullOrEmpty())
                {
                    context.Response.StatusCode = HttpStatusCode.MethodNotAllowed.ToInt();
                    return Task.FromResult(false);
                }

                if (op.ToLower() == "getjoblist")
                {
                    var jobList = GetRecurringJobs();
                    context.Response.StatusCode = HttpStatusCode.OK.ToInt();
                }
            }
            catch ()
            {

            }
        }

        /// <summary>
        /// 获取已经暂停的任务
        /// </summary>
        /// <returns></returns>
        private List<PauseRecurringJob> GetRecurringJobs()
        {
            var pauseList = new List<PauseRecurringJob>();
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobList = StorageConnectionExtensions.GetRecurringJobs(connection);
                jobList.ForEach(f =>
                {
                    var conts = connection.GetAllItemsFromSet($"JobPauseOf:{f.Id}");
                    if (conts.Contains("true"))
                        pauseList.Add(new PauseRecurringJob { Id = f.Id });
                });
            }

            return pauseList;
        }

        /// <summary>
        /// 添加周期性作业
        /// </summary>
        /// <param name="jobItem"></param>
        /// <returns></returns>
        public bool AddHttpRecurringJob(HttpJobItem jobItem)
        {
            var server = JobStorage.Current.GetMonitoringApi().Servers()
                .FirstOrDefault(w => w.Queues.Count > 0);
            if (server.IsNull())
                return false;

            var queues = server.Queues.ToList();
            if (!queues.Exists(p => p == jobItem.QueueName.ToLower()) || queues.Count == 0)
                return false;
            try
            {
                RecurringJob.AddOrUpdate(jobItem.JobName,()=>HttpJob.ex);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
