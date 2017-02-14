# Metrics

Metrics are reported in the [Prometheus wire format](http://prometheus.io). Under the hood, we're using the excellent [prometheus-net](https://github.com/andrasm/prometheus-net) client for .NET.

To add a Prometheus metrics scraping endpoint, register the HTTP handler like so:

```xml
<location path="metrics">
    <system.webServer>
        <handlers>
            <add name="ToolhouseMetricsHandler" path="*" verb="*" type="Toolhouse.Monitoring.MetricsEndpointHandler, Toolhouse.Monitoring"/>
        </handlers>
    </system.webServer>
</location>
```

To automatically generate metrics around incoming HTTP requests and application exceptions, add `Toolhouse.Monitoring.Modules.MetricsModule` to your `Web.config`:

```xml
<system.webServer>
    <modules runAllManagedModulesForAllRequests="true">
        <add name="ToolhouseMetricsModule" type="Toolhouse.Monitoring.Modules.MetricsModule, Toolhouse.Monitoring"/>
    </modules>
</system.webServer>
```

## Standard Metrics

Out of the box, [the metrics module](./Toolhouse.Metrics/Modules/MetricsModule.cs) provides the following metrics:

|              Metric             |     Type    |                                               Description                                                |
|---------------------------------|-------------|----------------------------------------------------------------------------------------------------------|
| `http_requests_total`           | `counter`   | Increments for each incoming request received.                                                           |
| `http_responses_total`          | `counter`   | Increments for each response returned, with `status` label for HTTP status.                              |
| `http_current_requests`         | `gauge`     | Number of requests *currently* being handled.                                                            |
| `http_request_duration_seconds` | `histogram` | Amount of time requests take to process.                                                                 |
| `errors_total`                  | `counter`   | Increments for each uncaught exception, with `error` label for Exception type (i.e. `System.Exception`). |

Note that all metrics are automatically given a `backend` label set to `Environment.MachineName`.

## Additional Metrics

The [`Toolhouse.Monitoring.Metrics`](../Toolhouse.Monitoring/Metrics.cs) class defines some static methods for working with other kinds of metrics, including:

- `IncrementEmailsSent(string)` - For tracking emails sent.
- `InstrumentApiCall(string, Func<bool>)` - For instrumenting calls to 3rd party APIs.
