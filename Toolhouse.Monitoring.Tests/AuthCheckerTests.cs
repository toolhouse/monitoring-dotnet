using System;

using NUnit.Framework;

namespace Toolhouse.Monitoring.Tests
{
    [TestFixture]
    public class AuthCheckerTests
    {
        [Test]
        public void TestBasicAuthSucceedsWithValidUserAndPassword()
        {
            var header = "Basic Zm9vOmJhcg==";
            var username = "foo";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsTrue(
                AuthChecker.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthSucceedsWithEmptyPassword()
        {
            var header = "Basic Zm9v";
            var username = "foo";
            var hashedPassword = "";

            Assert.IsTrue(
                AuthChecker.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthSucceedsWithNoUsernameConfigured()
        {
            var header = "";
            var username = "";
            var hashedPassword = "";
            Assert.IsTrue(
                AuthChecker.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthFailsWithInvalidUser()
        {
            var header = "Basic Zm9vOmJhcg==";
            var username = "obviously-wrong";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsFalse(
                AuthChecker.CheckAuthHeader(header, username, hashedPassword)
            );
        }

        [Test]
        public void TestBasicAuthFailsWithMissingHeader()
        {
            var header = "";
            var username = "foo";
            var hashedPassword = "fcde2b2edba56bf408601fb721fe9b5c338d10ee429ea04fae5511b68fbf8fb9";

            Assert.IsFalse(AuthChecker.CheckAuthHeader(header, username, hashedPassword));
        }
    }
}
