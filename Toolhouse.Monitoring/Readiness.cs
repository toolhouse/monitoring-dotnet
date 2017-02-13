using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Toolhouse.Monitoring.Dependencies;

namespace Toolhouse.Monitoring
{
    /// <summary>
    /// Place to register application dependencies and determine overall system readiness.
    /// </summary>
    public static class Readiness
    {
        private static readonly IList<IDependency> _dependencies = new List<IDependency>();

        public static void AddDependency(IDependency dependency)
        {
            lock (_dependencies)
            {
                _dependencies.Add(dependency);
            }
        }

        public static void AddDependency(string name, Func<bool> checker)
        {
            AddDependency(new FuncDependency(name, checker));
        }

        /// <summary>
        /// Adds a dependency on a (SQL) database using a named connection string
        /// specified in Web.config.
        /// </summary>
        public static void AddDatabaseDependency(string connectionStringName)
        {
            AddDatabaseDependency("database", connectionStringName);
        }

        public static void AddDatabaseDependency(string name, string connectionStringName)
        {
            AddDependency(new DatabaseDependency(name, connectionStringName));
        }

        /// <summary>
        /// Adds a dependency on a third party website.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="url"></param>
        public static void AddHttpDependency(string name, string url)
        {
            AddDependency(new HttpDependency(name, url));
        }

        /// <summary>
        /// Adds a dependency on the SMTP server configured in Web.config.
        /// </summary>
        public static void AddSmtpDependency()
        {
            AddDependency(new SmtpDependency("smtp"));
        }

        /// <summary>
        /// Checks the current status of all dependencies.
        /// </summary>
        public static IEnumerable<DependencyStatus> CheckDependencies()
        {
            List<IDependency> iterableDependencies;

            lock (_dependencies)
            {
                iterableDependencies = new List<IDependency>(_dependencies);
            }

            return iterableDependencies.AsParallel().Select(dep => {
                try
                {
                    return dep.Check();
                }
                catch (Exception ex)
                {
                    // Any exception during dependency check = the dependency is down.
                    return new DependencyStatus(
                        dep,
                        false,
                        string.Format("Exception during readiness check ({0}): {1}", ex.GetType().FullName, ex.Message)
                    );
                }
            });
        }
    }
}
