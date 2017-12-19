#if !NETCORE
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;

namespace Toolhouse.Monitoring.Handlers
{
    public class ReadinessEndpointHandler : AbstractHttpHandler
    {
        public override void ProcessRequestCore(HttpContext context)
        {
            if (!CheckAuthentication())
            {
                return;
            }

            var resp = context.Response;

            resp.StatusCode = 200;
            resp.ContentType = "application/json";
            resp.ContentEncoding = System.Text.Encoding.UTF8;

            var statuses = Readiness.CheckDependencies();
            var output = new Dictionary<string, object>();

            foreach (var status in statuses)
            {
                if (!status.IsReady)
                {
                    // The whole thing is no good.
                    resp.TrySkipIisCustomErrors = true;
                    resp.StatusCode = 503;
                }
                output[status.Dependency.Name] = new {
                    ready = status.IsReady,
                    message = status.Message,
                };
            }

            var serializer = new JsonSerializer();
            serializer.Serialize(resp.Output, output);
        }
    }
}
#endif
