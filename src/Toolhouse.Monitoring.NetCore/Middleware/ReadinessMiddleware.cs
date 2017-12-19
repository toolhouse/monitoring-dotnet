using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Toolhouse.Monitoring.Core;

namespace Toolhouse.Monitoring.NetCore.Middleware
{
    public class ReadinessMiddleware
    {
        public ReadinessMiddleware(RequestDelegate next, string username, string passwordSha256)
        {
            _username = username;
            _passwordSha256 = passwordSha256;
        }

        public async Task Invoke(HttpContext context)
        {
            HttpAuthChecker authChecker = new HttpAuthChecker(context, _username, _passwordSha256);
            if (!authChecker.CheckAuthentication())
            {
                return;
            }

            var resp = context.Response;

            resp.StatusCode = 200;
            resp.ContentType = "application/json";

            var statuses = Readiness.CheckDependencies();
            var output = new Dictionary<string, object>();

            foreach (var status in statuses)
            {
                if (!status.IsReady)
                {
                    resp.StatusCode = 503;
                }
                output[status.Dependency.Name] = new
                {
                    ready = status.IsReady,
                    message = status.Message,
                };
            }

            await resp.WriteAsync(JsonConvert.SerializeObject(output));
        }

        private readonly string _username;
        private readonly string _passwordSha256;
    }

    public static class ReadinessMiddlewareExtensions
    {
        public static IApplicationBuilder UseReadiness(this IApplicationBuilder builder, string username, string passwordSha256)
        {
            return builder.Map("/readiness", ab =>
                ab.UseMiddleware<ReadinessMiddleware>(username, passwordSha256));
        }
    }
}
