using NUnit.Framework;
using Prometheus.Advanced;
using System;
using System.Linq;

namespace Toolhouse.Monitoring.Tests
{
    [TestFixture]
    public class HttpRequestMetricsTests
    {
        [Test]
        public void TestRequestIsExecuted()
        {
            bool requestHasExecuted = false;
            Metrics.Instrument(string.Empty, () => { requestHasExecuted = true; });
            Assert.IsTrue(requestHasExecuted);

            requestHasExecuted = false;
            Metrics.Instrument(string.Empty, labels => { requestHasExecuted = true; });
            Assert.IsTrue(requestHasExecuted);

            requestHasExecuted = false;
            Metrics.Instrument(string.Empty, () => requestHasExecuted = true);
            Assert.IsTrue(requestHasExecuted);

            requestHasExecuted = false;
            Metrics.Instrument(string.Empty, labels => requestHasExecuted = true);
            Assert.IsTrue(requestHasExecuted);
        }

        [Test]
        public void TestInstrumentReturnsRequestValue()
        {
            int randomReturnValue = new Random().Next();
            Assert.AreEqual(randomReturnValue, Metrics.Instrument(string.Empty, () => randomReturnValue));
            Assert.AreEqual(randomReturnValue, Metrics.Instrument(string.Empty, labels => randomReturnValue));
        }

        [Test]
        public void TestRequestCounterIsIncremented()
        {
            string name = "example";
            TestCounterIsIncremented(string.Format("{0}_requests_total", name), () => { Metrics.Instrument(name, () => { }); });
            TestCounterIsIncremented(string.Format("{0}_requests_total", name), () => { Metrics.Instrument(name, labels => { }); });
            TestCounterIsIncremented(string.Format("{0}_requests_total", name), () => { Metrics.Instrument(name, () => true); });
            TestCounterIsIncremented(string.Format("{0}_requests_total", name), () => { Metrics.Instrument(name, labels => true); });
        }

        [Test]
        public void TestResponseCounterIsIncremented()
        {
            string name = "example";
            TestCounterIsIncremented(string.Format("{0}_responses_total", name), () => { Metrics.Instrument(name, () => { }); });
            TestCounterIsIncremented(string.Format("{0}_responses_total", name), () => { Metrics.Instrument(name, labels => { }); });
            TestCounterIsIncremented(string.Format("{0}_responses_total", name), () => { Metrics.Instrument(name, () => true); });
            TestCounterIsIncremented(string.Format("{0}_responses_total", name), () => { Metrics.Instrument(name, labels => true); });
        }

        [Test]
        public void TestCurrentRequestCounterIsIncrementedAndDecremented()
        {
            string name = "example";
            TestCurrentRequestCounterIsIncrementedAndDecremented(name, assert => { Metrics.Instrument(name, () => { assert(); }); });
            TestCurrentRequestCounterIsIncrementedAndDecremented(name, assert => { Metrics.Instrument(name, labels => { assert(); }); });
            TestCurrentRequestCounterIsIncrementedAndDecremented(name, assert => { Metrics.Instrument(name, () => { assert(); return true; }); });
            TestCurrentRequestCounterIsIncrementedAndDecremented(name, assert => { Metrics.Instrument(name, labels => { assert(); return true; }); });
        }

        [Test]
        public void TestHistogramIsCreated()
        {
            string name = "example";
            TestHistogramIsCreated(name, () => { Metrics.Instrument(name, () => { }); });
            TestHistogramIsCreated(name, () => { Metrics.Instrument(name, labels => { }); });
            TestHistogramIsCreated(name, () => { Metrics.Instrument(name, () => true); });
            TestHistogramIsCreated(name, () => { Metrics.Instrument(name, labels => true); });
        }

        [Test]
        public void TestResponsesTotalRecordsSuccessViaLabel()
        {
            string metricName = "test";
            Metrics.Instrument(metricName, () => { });

            DefaultCollectorRegistry registry = GetEmptyRegistry();
            string totalMetricName = string.Format("{0}_responses_total", metricName);

            Metrics.Instrument(metricName, () => { });

            var family = registry.CollectAll().First(x => x.name == totalMetricName);
            var metric = family.metric[0];
            var label = metric.label.Find(l => l.name == "success");

            Assert.IsNotNull(label, "success label not found");
            Assert.AreEqual("1", label.value);
        }

        [Test]
        public void TestResponsesTotalRecordsFailureViaLabel()
        {
            string metricName = "test";
            DefaultCollectorRegistry registry = GetEmptyRegistry();
            string totalMetricName = string.Format("{0}_responses_total", metricName);

            try
            {
                Metrics.Instrument(metricName, () =>
                {
                    throw new ApplicationException("Simulate error during request");
                });
                Assert.Fail("Metrics.Instrument should not swallow exceptions");
            }
            catch (ApplicationException)
            {
            }

            var family = registry.CollectAll().First(x => x.name == totalMetricName);
            var metric = family.metric[0];
            var label = metric.label.Find(l => l.name == "success");

            Assert.IsNotNull(label, "success label not found");
            Assert.AreEqual("0", label.value);
        }

        [Test]
        public void TestRequestDurationRecordsSuccessViaLabel()
        {
            string metricName = "test";
            string durationMetricName = string.Format("{0}_request_duration_seconds", metricName);
            DefaultCollectorRegistry registry = GetEmptyRegistry();

            Metrics.Instrument(metricName, () => { });

            var family = registry.CollectAll().First(x => x.name == durationMetricName);
            var metric = family.metric[1];
            var label = metric.label.Find(l => l.name == "success");

            Assert.IsNotNull(label, "success label not found");
            Assert.AreEqual("1", label.value);
        }

        [Test]
        public void TestRequestDurationRecordsFailureViaLabel()
        {
            string metricName = "test";
            string durationMetricName = string.Format("{0}_request_duration_seconds", metricName);
            DefaultCollectorRegistry registry = GetEmptyRegistry();

            try
            {

                Metrics.Instrument(metricName, () => { throw new ApplicationException("Simulate error during request"); });
                Assert.Fail("Metrics.Instrument should not swallow exceptions");
            }
            catch (ApplicationException)
            {
            }

            var family = registry.CollectAll().First(x => x.name == durationMetricName);
            var metric = family.metric[1];
            var label = metric.label.Find(l => l.name == "success");

            Assert.IsNotNull(label, "success label not found");
            Assert.AreEqual("0", label.value);
        }

        [Test]
        public void TestExceptionIsRethrown()
        {
            try
            {
                Metrics.Instrument("exception_rethrown", () =>
                {
                    throw new ApplicationException("Simulate exception during request");
                });
            }
            catch (ApplicationException)
            {
                return;
            }

            Assert.Fail("Metrics.Instrument should not swallow exceptions");
        }

        [Test]
        public void TestRequestCounterIsDecrementedOnException()
        {
            const string name = "request_counter_dec_on_exception";
            DefaultCollectorRegistry registry = GetEmptyRegistry();

            try
            {
                Metrics.Instrument(name, () =>
                {
                    throw new ApplicationException("Simulate exception during request.");
                });
            }
            catch (ApplicationException)
            {
                var family = registry.CollectAll().First(x => x.name == string.Format("{0}_current_requests", name));
                Assert.AreEqual(0, family.metric[0].gauge.value);
                return;
            }

            Assert.Fail("Metrics.Instrument should not swallow exceptions");
        }

        [Test]
        public void TestHistogramCreatedWhenExceptionThrown()
        {
            string name = "exception_thrown";
            try
            {
                TestHistogramIsCreated(name, () =>
                {
                    throw new ApplicationException("Simulate exception during request");
                });
            }
            catch (ApplicationException)
            {
                return;
            }
            Assert.Fail("Metrics.Instrument should not swallow exceptions");
        }


        private void TestHistogramIsCreated(string name, Action instrument)
        {
            DefaultCollectorRegistry registry = GetEmptyRegistry();
            instrument();
            var histogram = registry.CollectAll().First(x => x.name == string.Format("{0}_request_duration_seconds", name));
            Assert.IsTrue(histogram.metric.Count > 0);
        }

        private void TestCurrentRequestCounterIsIncrementedAndDecremented(string name, Action<Action> instrument)
        {
            string fullName = string.Format("{0}_current_requests", name);
            DefaultCollectorRegistry registry = GetEmptyRegistry();
            instrument(() =>
            {
                Assert.AreEqual(1, registry.CollectAll().First(x => x.name == fullName).metric[0].gauge.value);
            });
            Assert.AreEqual(0, registry.CollectAll().First(x => x.name == fullName).metric[0].gauge.value);
        }

        private void TestCounterIsIncremented(string name, Action instrument)
        {
            DefaultCollectorRegistry registry = GetEmptyRegistry();
            instrument();
            var counter = registry.CollectAll().First(x => x.name == name);
            Assert.AreEqual(1, counter.metric.Count);
        }

        private DefaultCollectorRegistry GetEmptyRegistry()
        {
            DefaultCollectorRegistry registry = DefaultCollectorRegistry.Instance;
            registry.Clear();
            Assert.IsFalse(registry.CollectAll().Any());
            return registry;
        }
    }
}
