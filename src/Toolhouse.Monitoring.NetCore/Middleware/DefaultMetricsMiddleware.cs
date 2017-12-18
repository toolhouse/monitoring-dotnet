using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Toolhouse.Monitoring.NetCore.Middleware
{
    /// <summary>
    /// Middleware that adds basic Prometheus instrumenting to incoming HTTP requests.
    /// </summary>
    public class DefaultMetricsMiddleware
    {
        public DefaultMetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Metrics.IncrementHttpRequestsCounter();
            Metrics.IncrementCurrentHttpRequestsGauge();

            try
            {
                await _next.Invoke(context);
            }
            catch (Exception ex)
            {
                Metrics.IncrementErrorsCounter(ex);
            }

            stopwatch.Stop();
            Metrics.DecrementCurrentHttpRequestsGauge();
            Metrics.IncrementHttpResponsesCounter(context.Response.StatusCode);
            Metrics.ObserveHttpRequestDuration(stopwatch.Elapsed);
        }

        private readonly RequestDelegate _next;
    }

    public static class DefaultMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder builder, string username, string passwordSha256)
        {
            return builder.UseMiddleware<DefaultMetricsMiddleware>();
        }
    }
}
