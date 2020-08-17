using Hangfire;
using Hangfire.Dashboard;
using Hangfire.HttpJob.Server;
using System;
using System.Reflection;
using UtilRepo;

namespace Hangfire.HttpJob
{
    public static class GlobalConfigurationExtension
    {
        public static IGlobalConfiguration UseHangfireHttpJob(this IGlobalConfiguration config,
            HangfireHttpJobOptions options = null)
        {
            if (options.IsNull())
                options = new HangfireHttpJobOptions();

            var assembly = typeof(HangfireHttpJobOptions).GetTypeInfo().Assembly;
            //处理http请求
            DashboardRoutes.Routes.Add("/httpjob", new HttpJobDispatcher(options));

            return config;
        }
    }
}
