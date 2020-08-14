using Hangfire;
using System;
using UtilRepo;

namespace Hangfire.HttpJob
{
    public static class GlobalConfigurationExtension
    {
        public static IGlobalConfiguration UseHangfireHttpJob(this IGlobalConfiguration config,
            HangfireHttpJobOptions options = null)
        { 
        if(options.IsNull())
        }
    }
}
