using HealthChecks.Uris;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobServer
{
    public static class UrisHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder,
            Uri uri, 
            HttpMethod httpMethod, 
            string name = null, 
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
        {
            builder.Services.AddHttpClient();
            UriHealthCheckOptions options = new UriHealthCheckOptions();
            options.AddUri(uri, (Action<IUriOptions>)null);
            options.UseHttpMethod(httpMethod);
            string registrationName = name ?? "uri-group";
            return builder.Add(new HealthCheckRegistration(registrationName, 
                (Func<IServiceProvider, IHealthCheck>)(sp => 
                (IHealthCheck)UrisHealthCheckBuilderExtensions.CreateHealthCheck(sp, registrationName, options)), 
                failureStatus, tags));
        }

        private static UriHealthCheck CreateHealthCheck(
      IServiceProvider sp,
      string name,
      UriHealthCheckOptions options)
        {
            IHttpClientFactory httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new UriHealthCheck(options, (Func<HttpClient>)(() => httpClientFactory.CreateClient(name)));
        }
    }
}
