using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Toolhouse.Monitoring
{
    public abstract class AuthChecker
    {
        private static readonly Regex sha256HashRegex = new Regex(@"^[0-9a-f]{64}$");

        private readonly string _username;
        private readonly string _passwordSha256;

        public AuthChecker(string username, string passwordSha256)
        {
            _username = username;
            _passwordSha256 = passwordSha256;
        }

        public bool CheckAuthentication()
        {
            var username = GetBasicAuthUsername();
            var passwordSha256 = GetBasicAuthPasswordSha256();

            if (username == "")
            {
                // No username is configured.
                return true;
            }

            if (!CheckAuthHeader(GetAuthHeader(), username, passwordSha256))
            {
                OnInvalidAuthHeader();
                return false;
            }

            return true;
        }

        public static bool CheckAuthHeader(string authHeader, string username, string passwordSha256)
        {
            var parsed = ParseBasicAuthHeader(authHeader);
            var headerUsername = parsed.Item1;
            var headerPassword = parsed.Item2;

            var usernameMatches = string.Equals(username, headerUsername, StringComparison.OrdinalIgnoreCase);
            var passwordConfigured = !string.IsNullOrEmpty(passwordSha256);
            var passwordMatches = (
                !passwordConfigured ||
                string.Equals(passwordSha256, HashPassword(headerPassword), StringComparison.OrdinalIgnoreCase)
            );

            return usernameMatches && passwordMatches;
        }

        /// <returns>
        /// A tuple with username and password elements.
        /// </returns>
        public static Tuple<string, string> ParseBasicAuthHeader(string header)
        {
            // RFC for Basic Auth: https://tools.ietf.org/html/rfc2617#section-2
            // Header looks like this:
            //  Authorization: Basic <user:pass base64 encoded>

            if (header == null || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return Tuple.Create("", "");
            }

            var userColonPassBytes = Convert.FromBase64String(header.Substring("Basic ".Length).Trim());
            var userColonPass = Encoding.UTF8.GetString(userColonPassBytes);

            var colonPos = userColonPass.IndexOf(':');
            var user = colonPos >= 0 ? userColonPass.Substring(0, colonPos) : userColonPass;
            var password = colonPos >= 0 ? userColonPass.Substring(colonPos + 1) : "";

            return Tuple.Create(user, password);
        }

        protected abstract string GetAuthHeader();

        /// <returns>
        /// Username used for HTTP basic auth.
        /// </returns>
        protected virtual string GetBasicAuthUsername()
        {
            return (_username ?? "").Trim();
        }

        /// <returns>
        /// Hex representation of the SHA256 hash of the password to use for basic auth, or
        /// an empty string ("") if no password is configured.
        /// </returns>
        protected virtual string GetBasicAuthPasswordSha256()
        {
            var hash = (_passwordSha256 ?? "")
                    .Trim()
                    .ToLower();

            if (hash == "")
            {
                // Assume no password configured.
                return "";
            }

            if (!sha256HashRegex.IsMatch(hash))
            {
                throw new Exception(string.Format(
                    "Invalid password hash specified for {0}: Should be a 64-character hex string",
                    hash
                ));
            }

            return hash;
        }

        protected virtual void OnInvalidAuthHeader()
        {
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = SHA256.Create().ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
