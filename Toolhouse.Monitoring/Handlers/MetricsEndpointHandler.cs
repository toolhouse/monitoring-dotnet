using System;
using System.Web;

using Prometheus;
using Prometheus.Advanced;

namespace Toolhouse.Monitoring.Handlers
{
    /// <summary>
    /// IHttpHandler implementation that provides a Prometheus metrics scraping endpoint.
    /// </summary>
    class MetricsEndpointHandler : AbstractHttpHandler
    {
        public override void ProcessRequest(HttpContext context)
        {
            if (!CheckAuthentication(context))
            {
                return;
            }

            var request = context.Request;
            var response = context.Response;

            // This adapts prometheus-net's internal HTTP serving code
            // Cribbed from http://www.erikojebo.se/Code/Details/792
            var acceptHeader = request.Headers.Get("Accept");
            var acceptHeaders = (acceptHeader ?? "").Split(',');

            response.ContentType = ScrapeHandler.GetContentType(acceptHeaders);
            response.ContentEncoding = System.Text.Encoding.UTF8;

            ScrapeHandler.ProcessScrapeRequest(
                DefaultCollectorRegistry.Instance.CollectAll(),
                response.ContentType,
                response.OutputStream
            );
        }
    }
}
