# Metrics

Metrics are reported in the [Prometheus wire format](http://prometheus.io). Under the hood, we're using the excellent [prometheus-net](https://github.com/andrasm/prometheus-net) client for .NET.

Out of the box, [the metrics module](./Toolhouse.Metrics/Modules/MetricsModule.cs) provides the following metrics:

|              Metric             |     Type    |                          Description                           |
|---------------------------------|-------------|----------------------------------------------------------------|
| `http_requests_total`           | `counter`   | Increments for each incoming request received.                 |
| `http_responses_total`          | `counter`   | Increments for each response returned, with label for `status` |
| `http_current_requests`         | `gauge`     |                                                                |
| `http_request_duration_seconds` | `histogram` |                                                                |
| `errors_total`                  | `counter`   | Increments for each uncaught exception.                        |

Note that all metrics are automatically given a `backend` label set to `Environment.MachineName`.

## Additional Metrics

The [`Toolhouse.Monitoring.Metrics`](../Toolhouse.Monitoring/Metrics.cs) class defines some static methods for working with other kinds of metrics, including:

- `IncrementEmailsSent(string)` - For tracking emails sent.
- `InstrumentApiCall(string, Func<bool>)` - For instrumenting calls to 3rd party APIs.
