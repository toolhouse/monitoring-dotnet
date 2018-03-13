using System;
using System.Diagnostics;
using System.Linq;

namespace Toolhouse.Monitoring
{
    /// <summary>
    /// Provides methods for working standard application metrics and creating new metrics.
    /// </summary>
    public static class Metrics
    {
        // Exception used as a compatibility shim for InstrumentApiCall().
        private class RequestFailedException : Exception
        {
        }

        /// <summary>
        /// ID of the metric used for the "current http requests" gauge.
        /// </summary>
        public const string CurrentHttpRequestsMetric = "http_current_requests";

        /// <summary>
        /// ID of metric used to track emails sent.
        /// </summary>
        public const string EmailsSentMetric = "emails_sent_total";

        /// <summary>
        /// ID of the standard error count metric.
        /// </summary>
        public const string ErrorsMetric = "errors_total";

        /// <summary>
        /// ID of the standard HTTP requests counter metric.
        /// </summary>
        public const string HttpRequestsMetric = "http_requests_total";

        /// <summary>
        /// ID of the standard HTTP responses counter metric.
        /// </summary>
        public const string HttpResponsesMetric = "http_responses_total";

        /// <summary>
        /// ID of standard "HTTP request duration" metric.
        /// </summary>
        public const string HttpRequestDurationMetric = "http_request_duration_seconds";

        /// <summary>
        /// Increments a standard "emails sent" counter.
        /// </summary>
        /// <param name="type">Type or template of email sent.</param>
        /// <param name="increment"></param>
        public static void IncrementEmailsSentCounter(string type, int increment = 1)
        {
            Prometheus.Metrics.CreateCounter(
                EmailsSentMetric,
                "",
                labelNames: new string[] { "backend", "type" }
            )
                .Labels(
                    GetBackend(),
                    type
                ).Inc();
        }

        /// <returns>
        /// ID of this server (used to distinguish metrics in load-balanced situations).
        /// </returns>
        public static string GetBackend()
        {
            return Environment.MachineName;
        }

        /// <summary>
        /// Increments the gauge tracking the current # of HTTP requests.
        /// </summary>
        public static void IncrementCurrentHttpRequestsGauge()
        {
            CreateCurrentHttpRequestsGauge().Inc();
        }

        public static void DecrementCurrentHttpRequestsGauge()
        {
            CreateCurrentHttpRequestsGauge().Dec();
        }

        /// <summary>
        /// Increments the error counter.
        /// </summary>
        /// <param name="type">String identifying the type of error.</param>
        public static void IncrementErrorsCounter(string type, int increment = 1)
        {
            Prometheus.Metrics.CreateCounter(
                ErrorsMetric,
                "Errors",
                labelNames: new string[] { "error", "backend" }
            )
                .Labels(
                    type,
                    GetBackend()
                ).Inc(increment);
        }

        /// <param name="ex">
        /// Exception encountered.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
        public static void IncrementErrorsCounter(Exception ex)
        {
            if (ex == null)
            {
                throw new ArgumentNullException("ex");
            }

            IncrementErrorsCounter(ex.GetType().ToString());
        }

        /// <summary>
        /// Increments the standard HTTP requests counter.
        /// </summary>
        public static void IncrementHttpRequestsCounter()
        {
            Prometheus.Metrics.CreateCounter(
                HttpRequestsMetric,
                "HTTP requests",
                labelNames: new string[] { "backend" }
            )
                .Labels(
                    GetBackend()
                ).Inc();
        }

        /// <summary>
        /// Increments the standard HTTP responses counter.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        public static void IncrementHttpResponsesCounter(int statusCode)
        {
            Prometheus.Metrics.CreateCounter(
                HttpResponsesMetric,
                "HTTP responses",
                labelNames: new string[] { "backend", "status" }
            )
                .Labels(
                    GetBackend(),
                    statusCode.ToString()
                ).Inc();
        }

        /// <summary>
        /// Logs an HTTP request duration metric.
        /// </summary>
        public static void ObserveHttpRequestDuration(TimeSpan duration)
        {
            Prometheus.Metrics.CreateHistogram(
                HttpRequestDurationMetric,
                "Request duration (in seconds).",
                buckets: new double[] { 0.1, 0.25, 0.5, 0.75, 1, 2, 3, 5, 10 },
                labelNames: new string[] { "backend" }
            )
                .Labels(
                    GetBackend()
                ).Observe(duration.TotalSeconds);
        }

        /// <summary>
        /// Instruments a block of code that makes a request to an external API (e.g. via REST / SOAP).
        /// </summary>
        /// <param name="name">Descriptive name of thing being done. Should be a singular noun, e.g. "salesforce".</param>
        /// <param name="makeRequest">Code to run. Should return true for success, false for failure.</param>
        [Obsolete("Use Metrics.Instrument instead.")]
        public static void InstrumentApiCall(string name, Func<bool> makeRequest)
        {
            try
            {
                Instrument(name, () =>
                {
                    var success = makeRequest();
                    if (!success)
                    {
                        throw new RequestFailedException();
                    }
                });
            }
            catch (RequestFailedException)
            {
                // This exception is used to force Instrument() into recording the failure
            }
        }

        /// <summary>
        /// Instruments a block of code that makes a request to an external API (e.g. via REST / SOAP).
        /// </summary>
        /// <param name="name">Descriptive name of thing being done. Should be a singular noun, e.g. "salesforce".</param>
        /// <param name="request">An action which will perform the request</param>
        public static void Instrument(string name, Action request)
        {
            InstrumentWithMetrics(name, requestMetrics =>
            {
                requestMetrics.PerformRequest(request);
            });
        }

        /// <summary>
        /// Instruments a block of code that makes a request to an external API (e.g. via REST / SOAP).
        /// </summary>
        /// <param name="name">Descriptive name of thing being done. Should be a singular noun, e.g. "salesforce".</param>
        /// <param name="request">An action which will perform the request</param>
        /// <remarks>A mutable Labels instance will be provided to the request.</remarks>
        public static void Instrument(string name, Action<Labels> request)
        {
            InstrumentWithMetrics(name, requestMetrics =>
            {
                requestMetrics.PerformRequest(() =>
                {
                    request(requestMetrics.Labels);
                });
            });
        }

        /// <summary>
        /// Instruments a block of code that makes a request to an external API (e.g. via REST / SOAP).
        /// </summary>
        /// <param name="name">Descriptive name of thing being done. Should be a singular noun, e.g. "salesforce".</param>
        /// <param name="request">An action which will perform the request</param>
        /// <returns>The return value of the request.</returns>
        public static T Instrument<T>(string name, Func<T> request)
        {
            return InstrumentWithMetrics(name, requestMetrics =>
            {
                return requestMetrics.PerformRequest(request);
            });
        }

        /// <summary>
        /// Instruments a block of code that makes a request to an external API (e.g. via REST / SOAP).
        /// </summary>
        /// <param name="name">Descriptive name of thing being done. Should be a singular noun, e.g. "salesforce".</param>
        /// <param name="request">An action which will perform the request</param>
        /// <remarks>A mutable Labels instance will be provided to the request.</remarks>
        /// <returns>The return value of the request.</returns>
        public static T Instrument<T>(string name, Func<Labels, T> request)
        {
            return InstrumentWithMetrics(name, requestMetrics =>
            {
                return requestMetrics.PerformRequest(() =>
                {
                    return request(requestMetrics.Labels);
                });
            });
        }

        private static void InstrumentWithMetrics(string name, Action<HttpRequestMetrics> instrumentWithMetrics)
        {
            InstrumentWithMetrics(name, requestMetrics =>
            {
                instrumentWithMetrics(requestMetrics);
                return true;
            });
        }

        private static T InstrumentWithMetrics<T>(string name, Func<HttpRequestMetrics, T> instrumentWithMetrics)
        {
            if (name.Any(char.IsWhiteSpace))
            {
                throw new ArgumentException("Parameter must be a single word.", "name");
            }

            HttpRequestMetrics requestMetrics = new HttpRequestMetrics(name);
            return instrumentWithMetrics(requestMetrics);
        }

        private static Prometheus.Gauge.Child CreateCurrentHttpRequestsGauge()
        {
            return Prometheus.Metrics.CreateGauge(
                CurrentHttpRequestsMetric,
                "Number of HTTP requests in progress",
                labelNames: new string[]
                {
                    "backend",
                }
            )
                .Labels(GetBackend());
        }
    }
}
