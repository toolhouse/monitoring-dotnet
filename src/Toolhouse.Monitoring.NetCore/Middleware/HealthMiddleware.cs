using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Toolhouse.Monitoring.NetCore.Middleware
{
    /// <summary>
    /// Basic healthcheck middleware. Just returns 200.
    /// </summary>
    public class HealthMiddleware
    {
        public HealthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("OK");

            await _next.Invoke(context);
        }

        private readonly RequestDelegate _next;
    }

    public static class HealthMiddlewareExtensions
    {
        public static IApplicationBuilder UseHealthcheck(this IApplicationBuilder builder)
        {
            return builder.Map("/health", ab => ab.UseMiddleware<HealthMiddleware>());
        }
    }
}
