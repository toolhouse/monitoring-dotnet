using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Prometheus;
using Prometheus.Advanced;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Toolhouse.Monitoring.NetCore.Middleware
{
    /// <summary>
    /// Middleware that provides a Prometheus metrics scraping endpoint.
    /// </summary>
    public class CustomMetricsMiddleware
    {
        public CustomMetricsMiddleware(RequestDelegate next, string username, string passwordSha256)
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

            var request = context.Request;
            var response = context.Response;

            // This adapts prometheus-net's internal HTTP serving code
            // Cribbed from http://www.erikojebo.se/Code/Details/792
            // Further edits were made to adapt to .NET Core
            var acceptHeader = (string) request.Headers[ "Accept"];
            var acceptHeaders = (acceptHeader ?? "").Split(',');

            response.ContentType = ScrapeHandler.GetContentType(acceptHeaders);

            string output;

            using (var stream = new MemoryStream())
            {
                ScrapeHandler.ProcessScrapeRequest(
                    DefaultCollectorRegistry.Instance.CollectAll(),
                    response.ContentType,
                    stream
                );
                output = Encoding.UTF8.GetString(stream.ToArray());
            }

            await response.WriteAsync(output);
        }

        private readonly string _username;
        private readonly string _passwordSha256;
    }

    public static class CustomMetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMetrics(this IApplicationBuilder builder, string username, string passwordSha256)
        {
            return builder.Map("/metrics", ab =>
                ab.UseMiddleware<CustomMetricsMiddleware>(username, passwordSha256));
        }
    }
}
