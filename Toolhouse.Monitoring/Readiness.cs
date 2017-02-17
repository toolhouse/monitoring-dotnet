using System;
using System.Collections.Generic;
using System.Linq;

using Toolhouse.Monitoring.Dependencies;

namespace Toolhouse.Monitoring
{
    /// <summary>
    /// Place to register application dependencies and determine overall system readiness.
    /// </summary>
    public static class Readiness
    {
        private static readonly IList<IDependency> Dependencies = new List<IDependency>();

        public static void AddDependency(IDependency dependency)
        {
            lock (Dependencies)
            {
                Dependencies.Add(dependency);
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
        /// <param name="name">Name of the dependency.</param>
        /// <param name="url">URL to request during dependency check.</param>
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

        public static IEnumerable<DependencyStatus> CheckDependencies()
        {
            List<IDependency> iterableDependencies;

            lock (Dependencies)
            {
                iterableDependencies = new List<IDependency>(Dependencies);
            }

            return iterableDependencies.AsParallel().Select(dep =>
            {
                try
                {
                    return dep.Check();
                }
                catch (Exception ex)
                {
                    // Any exception during dependency check = the dependency is down.
                    var message = string.Format("Exception during readiness check ({0}): {1}", ex.GetType().FullName, ex.Message);
                    return new DependencyStatus(dep, false, message);
                }
            });
        }
    }
}
