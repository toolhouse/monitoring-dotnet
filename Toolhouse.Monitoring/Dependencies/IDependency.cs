using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolhouse.Monitoring.Dependencies
{
    public interface IDependency
    {
        /// <summary>
        /// Short name of this dependency.
        /// </summary>
        string Name { get; }

        /// <summary />
        /// <returns></returns>
        DependencyStatus Check();
    }
}
