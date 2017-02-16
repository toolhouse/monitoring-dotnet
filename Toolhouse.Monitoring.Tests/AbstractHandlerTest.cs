using System;

using NUnit.Framework;

using Toolhouse.Monitoring.Handlers;

namespace Toolhouse.Monitoring.Tests
{
    [TestFixture]
    public class AbstractHandlerTest
    {
        [Test]
        public void TestBasicAuthSucceedsWithValidUserAndPassword()
        {
            var header = "Basic Zm9vOmJhcg==";
            var username = "foo";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsTrue(
                AbstractHttpHandler.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthSucceedsWithEmptyPassword()
        {
            var header = "Basic Zm9v";
            var username = "foo";
            var hashedPassword = "";

            Assert.IsTrue(
                AbstractHttpHandler.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthSucceedsWithNoUsernameConfigured()
        {
            var header = "";
            var username = "";
            var hashedPassword = "";
            Assert.IsTrue(
                AbstractHttpHandler.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthFailsWithInvalidUser()
        {
            var header = "Basic Zm9vOmJhcg==";
            var username = "obviously-wrong";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsFalse(
                AbstractHttpHandler.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthFailsWithMissingHeader()
        {
            var header = "";
            var username = "foo";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsFalse(AbstractHttpHandler.CheckAuthHeader(header, username, hashedPassword));
        }
    }
}
