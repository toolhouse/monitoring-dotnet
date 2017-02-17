using System;
using System.Net;

namespace Toolhouse.Monitoring.Dependencies
{
    /// <summary>
    /// Dependency that makes an HTTP GET request for a URL.
    /// </summary>
    class HttpDependency : IDependency
    {
        public HttpDependency(string name, string url, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            this.Name = name;
            this.Url = url;
            this.ExpectedStatusCode = expectedStatusCode;
        }

        public HttpStatusCode ExpectedStatusCode
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Url
        {
            get;
            private set;
        }

        public DependencyStatus Check()
        {
            var req = WebRequest.Create(this.Url);
            var resp = (HttpWebResponse) req.GetResponse();

            var message = string.Format(
                "Request returned HTTP status {0}: {1}",
                (int)resp.StatusCode,
                this.Url
            );

            if (resp.StatusCode != this.ExpectedStatusCode)
            {
                throw new Exception(string.Format("{0} (expected {1})", message, this.ExpectedStatusCode));
            }
            return new DependencyStatus(this, true, message);
        }
    }
}
