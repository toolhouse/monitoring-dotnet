namespace Toolhouse.Monitoring.Dependencies
{
    public interface IDependency
    {
        /// <summary>
        /// Gets the short name of this dependency.
        /// </summary>
        string Name { get; }

        /// <summary />
        /// <returns>The current status of the dependency.</returns>
        DependencyStatus Check();
    }
}
