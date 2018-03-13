#if NETCORE
using Microsoft.AspNetCore.Http;

namespace Toolhouse.Monitoring
{
    public class HttpAuthChecker : AuthChecker
    {
        public HttpAuthChecker(HttpContext context, string username, string passwordSha256)
            : base(username, passwordSha256)
        {
            _context = context;
        }

        protected override string GetAuthHeader()
        {
            return _context.Request.Headers["Authorization"];
        }

        protected override void OnInvalidAuthHeader()
        {
            _context.Response.StatusCode = 403;
        }

        HttpContext _context;
    }
}
#endif
