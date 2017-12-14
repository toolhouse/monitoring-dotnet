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
