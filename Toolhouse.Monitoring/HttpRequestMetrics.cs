using Prometheus;
using System;
using System.Diagnostics;
using System.Linq;

namespace Toolhouse.Monitoring
{
    /// <summary>
    /// Provides methods for instrumenting requests to an external API (e.g. via REST / SOAP).
    /// </summary>
    public sealed class HttpRequestMetrics
    {
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
                    request(requestMetrics.m_labels);
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
                    return request(requestMetrics.m_labels);
                });
            });
        }

        private HttpRequestMetrics(string name)
        {
            m_labels = new Labels();
            m_name = name;
            m_stopwatch = new Stopwatch();
            m_currentRequests = Prometheus.Metrics.CreateGauge(string.Format("{0}_current_requests", m_name), string.Empty, m_labels.BackendLabelKey);
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

        private void PerformRequest(Action request)
        {
            PerformRequest(() =>
            {
                request();
                return true;
            });
        }

        private T PerformRequest<T>(Func<T> request)
        {
            StartGatheringMetrics();
            T result = request();
            StopGatheringMetrics();
            RecordMetrics();
            return result;
        }

        private void StartGatheringMetrics()
        {
            Prometheus.Metrics.CreateCounter(string.Format("{0}_requests_total", m_name), string.Empty, m_labels.BackendLabelKey)
                .Labels(m_labels[m_labels.BackendLabelKey]).Inc();
            m_currentRequests.Labels(m_labels[m_labels.BackendLabelKey]).Inc();
            m_stopwatch.Start();
        }

        private void StopGatheringMetrics()
        {
            m_stopwatch.Stop();
            m_currentRequests.Labels(m_labels[m_labels.BackendLabelKey]).Dec();
        }

        private void RecordMetrics()
        {
            Prometheus.Metrics.CreateCounter(string.Format("{0}_responses_total", m_name), string.Empty, m_labels.Keys)
                .Labels(m_labels.Values).Inc();
            Prometheus.Metrics.CreateHistogram(string.Format("{0}_request_duration_seconds", m_name), string.Empty, null, m_labels.Keys)
                .Labels(m_labels.Values).Observe(m_stopwatch.Elapsed.TotalSeconds);
        }

        readonly Labels m_labels;
        readonly string m_name;
        readonly Stopwatch m_stopwatch;
        readonly Gauge m_currentRequests;
    }
}
