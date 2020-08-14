using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Heartbeat;
using Hangfire.MySql;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace JobServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            if (HangfireSettings.Instance.UseRedis)
            {
                Redis = ConnectionMultiplexer.Connect(HangfireSettings.Instance.HangfireRedisConnectionString);
            }
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //健康检查地址添加
            var hostList = HangfireSettings.Instance.HostServers;
            //添加健康检查地址
            hostList.ForEach(host =>
            {
                services.AddHealthChecks()
                    .AddUrlGroup(new Uri(host.Uri), host.httpMethod.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, $"{host.Uri}");
            });


            //redis集群检查地址添加
            var redislist = HangfireSettings.Instance.HangfireRedisConnectionString.Split(",").ToList();
            redislist.ForEach(
                k =>
                {
                    if (k.Contains(":"))
                    {
                        services.AddHealthChecks().AddRedis(k, $"Redis: {k}");
                    }
                }
                );
            services.AddHangfire(config =>
            {
                //使用服务器资源监视
                config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));

                if (HangfireSettings.Instance.UseMySql)
                {
                    _ = config.UseMySqlStorage(HangfireSettings.Instance.HangfireMysqlConnectionString,
                        new MySqlStorageOptions
                        {
                            //每隔一小时检查过期job
                            JobExpirationCheckInterval = TimeSpan.FromHours(1),
                            QueuePollInterval = TimeSpan.FromSeconds(1)
                        })
                    .usehan
                    ;
                }
            });

        }
        public static ConnectionMultiplexer Redis;
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
