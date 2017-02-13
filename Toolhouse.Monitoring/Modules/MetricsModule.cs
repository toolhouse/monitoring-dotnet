using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;

namespace Toolhouse.Monitoring.Modules
{
    /// <summary>
    /// HTTP module that adds basic Prometheus instrumenting to incoming HTTP requests.
    /// It also adds an endpoint at "/metrics" for Prometheus servers to scrape.
    /// </summary>
    class MetricsModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication application)
        {
            application.BeginRequest += Application_BeginRequest;
            application.EndRequest += Application_EndRequest;
            application.Error += Application_Error;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;

            // Initialize request timing
            var stopwatch = new Stopwatch();
            application.Context.Items["Toolhouse.Stopwatch"] = stopwatch;
            stopwatch.Start();

            Metrics.IncrementHttpRequestsCounter();
            Metrics.IncrementCurrentHttpRequestsGauge();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var stopwatch = application.Context.Items["Toolhouse.Stopwatch"] as Stopwatch;
            if (stopwatch != null)
            {
                stopwatch.Stop();
            }

            Metrics.DecrementCurrentHttpRequestsGauge();
            Metrics.IncrementHttpResponsesCounter(application.Response.StatusCode);

            if (stopwatch != null)
            {
                Metrics.ObserveHttpRequestDuration(stopwatch.Elapsed);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var ex = application.Server.GetLastError();

            if (ex != null)
            {
                Metrics.IncrementErrorsCounter(ex);
        }
    }
}
}
