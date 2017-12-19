using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolhouse.Monitoring.Core.Dependencies
{
    /// <summary>
    /// Wrapper that allows using a lambda as a dependency check.
    /// </summary>
    internal class FuncDependency : IDependency
    {
        private Func<bool> _checker;

        public FuncDependency(string name, Func<bool> checker)
        {
            this.Name = name;
            _checker = checker;
        }

        public string Name
        {
            get;
            private set;
        }

        public DependencyStatus Check()
        {
            var ready = _checker();
            return new DependencyStatus(this, ready, "");
        }
    }
}
