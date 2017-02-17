using System;

namespace Toolhouse.Monitoring.Dependencies
{
    /// <summary>
    /// Wrapper that allows using a lambda as a dependency check.
    /// </summary>
    internal class FuncDependency : IDependency
    {
        private Func<bool> checker;

        public FuncDependency(string name, Func<bool> checker)
        {
            this.Name = name;
            this.checker = checker;
        }

        public string Name
        {
            get;
            private set;
        }

        public DependencyStatus Check()
        {
            var ready = checker();
            return new DependencyStatus(this, ready, "");
        }
    }
}
