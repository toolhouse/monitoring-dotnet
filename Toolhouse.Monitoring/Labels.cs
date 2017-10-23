using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Toolhouse.Monitoring
{
    public sealed class Labels
    {
        private const string c_backendLabelKey = "backend";
        private const string c_statusCodeLabelKey = "statusCode";
        private const string c_successLabelKey = "success";

        private readonly Dictionary<string, string> m_labels;

        public Labels()
        {
            m_labels = new Dictionary<string, string>();
            m_labels[c_backendLabelKey] = Metrics.GetBackend();
        }

        public string BackendLabelKey
        {
            get { return c_backendLabelKey; }
        }

        public string[] Keys
        {
            get { return m_labels.Keys.ToArray(); }
        }

        public string[] Values
        {
            get { return m_labels.Values.ToArray(); }
        }

        public string this[string key]
        {
            get { return m_labels[key]; }
            set
            {
                if (key == c_backendLabelKey)
                {
                    throw new InvalidOperationException("Cannot change the value of the backend label.");
                }

                m_labels[key] = value;
            }
        }

        public void AddStatusCode(HttpStatusCode statusCode)
        {
            this[c_statusCodeLabelKey] = ((int) statusCode).ToString();
        }

        public void AddSuccess()
        {
            this[c_successLabelKey] = "1";
        }

        public void AddFailure()
        {
            this[c_successLabelKey] = "0";
        }
    }
}
