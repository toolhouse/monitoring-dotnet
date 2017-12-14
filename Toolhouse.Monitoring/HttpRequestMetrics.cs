using Prometheus;
using System;
using System.Diagnostics;

namespace Toolhouse.Monitoring
{
    /// <summary>
    /// Provides methods for instrumenting requests to an external API (e.g. via REST / SOAP).
    /// </summary>
    public class HttpRequestMetrics
    {
        private readonly Labels m_labels;
        private readonly string m_name;
        private readonly Stopwatch m_stopwatch;
        private readonly Gauge m_currentRequests;

        /// <summary>
        /// Creates an instance of HttpRequestMetrics
        /// </summary>
        /// <param name="name"></param>
        public HttpRequestMetrics(string name)
        {
            m_labels = new Labels();
            m_name = name;
            m_stopwatch = new Stopwatch();
            m_currentRequests = Prometheus.Metrics.CreateGauge(string.Format("{0}_current_requests", m_name), string.Empty, m_labels.BackendLabelKey);
        }

        public Labels Labels
        {
            get { return m_labels; }
        }

        public void PerformRequest(Action request)
        {
            PerformRequest(() =>
            {
                request();
                return true;
            });
        }

        public T PerformRequest<T>(Func<T> request)
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
    }
}
