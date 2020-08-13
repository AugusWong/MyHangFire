using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UtilRepo;

namespace Hangfire.Dashboard.BasicAuthorization
{
    public class BasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly BasicAuthAuthorizationFilterOptions _options;

        public BasicAuthAuthorizationFilter() : this(new BasicAuthAuthorizationFilterOptions())
        {


        }
        public BasicAuthAuthorizationFilter(BasicAuthAuthorizationFilterOptions options)
        {
            _options = options;
        }
        private bool Challenge(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            return false;
        }
        public bool Authorize([NotNull] DashboardContext _context)
        {
            var context = _context.GetHttpContext();
            if (_options.SslRedirect && context.Request.Scheme != "https")
            {
                string redirectUri = new UriBuilder("https", context.Request.Host.ToString(), 443, context.Request.Path).ToString();

                context.Response.StatusCode = 301;
                context.Response.Redirect(redirectUri);
                return false;
            }

            if (_options.RequireSsl && context.Request.IsHttps)
            {
                return false;
            }

            string header = context.Request.Headers["Authorization"];

            if(header.IsNotNullOrWhiteSpace())
            {
                AuthenticationHeaderValue authvalues = AuthenticationHeaderValue.Parse(header);
                if("Basic".Equals(authvalues.Scheme,StringComparison.OrdinalIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authvalues.Parameter));
                    var parts = parameter.Split(':');

                    if (parts.Length > 1)
                    {
                        string login = parts[0];
                        string password = parts[1];

                        if (login.IsNotNullOrWhiteSpace()&& password.IsNotNullOrWhiteSpace())
                        {
                            return _options
                                .Users
                                .Any(user => user.Validate(login, password, _options.LoginCaseSensitive))
                                   || Challenge(context);
                        }
                    }
                }
            }

            return Challenge(context);
        }
    }
}
