using NUnit.Framework;
using System.Net;

namespace Toolhouse.Monitoring.Tests
{
    [TestFixture]
    public class LabelsTests
    {
        [Test]
        public void TestIsInitializedWithBackendLabel()
        {
            Labels labels = new Labels();
            Assert.AreEqual(1, labels.Keys.Length);
            Assert.AreEqual(1, labels.Values.Length);
            Assert.AreEqual(labels.BackendLabelKey, labels.Keys[0]);
        }

        [Test]
        public void TestKeysAndValuesMirrorInsertedKeysAndValues()
        {
            Labels labels = new Labels();
            string key1 = "foo", value1 = "bar";
            string key2 = "baz", value2 = "qux";
            labels[key1] = value1;
            labels[key2] = value2;
            CollectionAssert.AreEquivalent(new[] { labels.BackendLabelKey, key1, key2 }, labels.Keys);
            CollectionAssert.AreEquivalent(new[] { labels[labels.BackendLabelKey], value1, value2 }, labels.Values);
        }

        [Test]
        public void TestInsertingHttpStatusCodeResultsInStringInsert()
        {
            Labels labels = new Labels();
            string statusCodeKey = "statusCode";
            HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            CollectionAssert.DoesNotContain(labels.Keys, statusCodeKey);
            labels.AddStatusCode(statusCode);
            Assert.AreEqual(((int) statusCode).ToString(), labels[statusCodeKey]);
        }

        [Test]
        public void TestSubsequentStatusCodesOverwritePreviousOnes()
        {
            Labels labels = new Labels();
            string statusCodeKey = "statusCode";
            CollectionAssert.DoesNotContain(labels.Keys, statusCodeKey);
            labels.AddStatusCode(HttpStatusCode.OK);
            labels.AddStatusCode(HttpStatusCode.BadRequest);
            Assert.AreEqual(((int) HttpStatusCode.BadRequest).ToString(), labels[statusCodeKey]);
            CollectionAssert.DoesNotContain(labels.Values, ((int) HttpStatusCode.OK).ToString());
        }

        [Test]
        public void TestReportingSuccessResultsInStringInsert()
        {
            Labels labels = new Labels();
            string successCodeKey = "success";
            CollectionAssert.DoesNotContain(labels.Keys, successCodeKey);
            labels.AddSuccess();
            Assert.AreEqual("1", labels[successCodeKey]);
        }

        [Test]
        public void TestReportingFailureResultsInStringInsert()
        {
            Labels labels = new Labels();
            string successCodeKey = "success";
            CollectionAssert.DoesNotContain(labels.Keys, successCodeKey);
            labels.AddFailure();
            Assert.AreEqual("0", labels[successCodeKey]);
        }

        [Test]
        public void TestSuccessAndFailureOverwriteEachOther()
        {
            Labels labels = new Labels();
            string successCodeKey = "success";
            CollectionAssert.DoesNotContain(labels.Keys, successCodeKey);

            labels.AddSuccess();
            Assert.AreEqual("1", labels[successCodeKey]);

            labels.AddFailure();
            Assert.AreEqual("0", labels[successCodeKey]);
            CollectionAssert.DoesNotContain(labels.Values, "1");

            labels.AddSuccess();
            Assert.AreEqual("1", labels[successCodeKey]);
            CollectionAssert.DoesNotContain(labels.Values, "0");
        }
    }
}
