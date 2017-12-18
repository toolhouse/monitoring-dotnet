using System;
using System.Configuration;
using System.Web;
using System.Text.RegularExpressions;

namespace Toolhouse.Monitoring.Handlers
{
    /// <summary>
    /// Base for implementing monitoring HTTP handlers with support for basic authentication.
    /// </summary>
    public abstract class AbstractHttpHandler : AuthChecker, IHttpHandler
    {
        private static readonly Regex sha256HashRegex = new Regex(@"^[0-9a-f]{64}$");

        private HttpContext _context;

        public AbstractHttpHandler()
            : base(ConfigurationManager.AppSettings["Toolhouse.Monitoring.Username"],
                  ConfigurationManager.AppSettings["Toolhouse.Monitoring.PasswordSha256"])
        {
        }

        public virtual bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            _context = context;
            ProcessRequestCore(_context);
        }

        public abstract void ProcessRequestCore(HttpContext context);

        protected override string GetAuthHeader()
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }

            return _context.Request.Headers["Authorization"];
        }

        protected override void OnInvalidAuthHeader()
        {
            if (_context == null)
            {
                throw new InvalidOperationException();
            }

            _context.Response.StatusCode = 403;
        }
    }
}
