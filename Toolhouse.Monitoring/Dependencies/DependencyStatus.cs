using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolhouse.Monitoring.Dependencies
{
    /// <summary>
    /// Structure describing the current status of an IDependency.
    /// </summary>
    public class DependencyStatus
    {
        private DependencyStatus()
        {
        }

        public DependencyStatus(IDependency dependency, bool ready, string message)
        {
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }

            this.Dependency = dependency;
            this.IsReady = ready;
            this.Message = message ?? "";
        }

        public IDependency Dependency
        {
            get;
            private set;
        }

        public bool IsReady
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }
    }
}
