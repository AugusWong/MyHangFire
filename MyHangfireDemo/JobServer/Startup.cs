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
            //��������ַ���
            var hostList = HangfireSettings.Instance.HostServers;
            //��ӽ�������ַ
            hostList.ForEach(host =>
            {
                services.AddHealthChecks()
                    .AddUrlGroup(new Uri(host.Uri), host.httpMethod.ToLower() == "post" ? HttpMethod.Post : HttpMethod.Get, $"{host.Uri}");
            });


            //redis��Ⱥ����ַ���
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
                //ʹ�÷�������Դ����
                config.UseHeartbeatPage(checkInterval: TimeSpan.FromSeconds(1));

                if (HangfireSettings.Instance.UseMySql)
                {
                    _ = config.UseMySqlStorage(HangfireSettings.Instance.HangfireMysqlConnectionString,
                        new MySqlStorageOptions
                        {
                            //ÿ��һСʱ������job
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
