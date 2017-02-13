using System;
using System.Web;

namespace Toolhouse.Monitoring.Handlers
{
    /// <summary>
    /// Basic healthcheck endpoint handler. Just returns 200.
    /// </summary>
    public class HealthEndpointHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true;  }
        }

        public void ProcessRequest(HttpContext context)
        {
            var resp = context.Response;
            resp.StatusCode = 200;
            resp.Write("OK");
        }
    }
}
