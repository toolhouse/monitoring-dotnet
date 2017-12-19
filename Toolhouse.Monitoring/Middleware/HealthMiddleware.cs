#if NETCORE
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Toolhouse.Monitoring.Middleware
{
    /// <summary>
    /// Basic healthcheck middleware. Just returns 200.
    /// </summary>
    public class HealthMiddleware
    {
        public HealthMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");
        }
    }

    public static class HealthMiddlewareExtensions
    {
        public static IApplicationBuilder UseHealthcheck(this IApplicationBuilder builder)
        {
            return builder.Map("/health", ab => ab.UseMiddleware<HealthMiddleware>());
        }
    }
}
#endif
