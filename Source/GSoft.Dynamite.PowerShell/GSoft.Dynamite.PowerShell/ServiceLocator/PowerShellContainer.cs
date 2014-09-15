using Autofac;
using GSoft.Dynamite.ServiceLocator;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.PowerShell.Unity
{
    /// <summary>
    /// The power shell container.
    /// </summary>
    public static class PowerShellContainer
    {
        /// <summary>
        /// The application name
        /// </summary>
        private const string AppName = "GSoft.Dynamite.PowerShell";

        /// <summary>
        /// The Service locator to scan the GAC with the specific AppName
        /// </summary>
        private static ISharePointServiceLocator serviceLocator = new SharePointServiceLocator(AppName);

        /// <summary>
        /// Dependency injection container instance
        /// </summary>
        public static ILifetimeScope Current
        {
            get
            {
                return serviceLocator.Current;
            }
        }

        /// <summary>
        /// Method to create a new LifeTime scope used for the lifetime of the container objects within a feature event
        /// </summary>
        /// <param name="feature">The feature that define the scope</param>
        /// <returns>A LifeTimeScope</returns>
        public static ILifetimeScope BeginLifetimeScope(SPFeature feature)
        {
            return serviceLocator.BeginLifetimeScope(feature);
        }

        /// <summary>
        /// Method to create a new LifeTime scope used for the lifetime of the container objects within a web
        /// </summary>
        /// <param name="web">The web that define the scope</param>
        /// <returns>A LifeTimeScope</returns>
        public static ILifetimeScope BeginLifetimeScope(SPWeb web)
        {
            return serviceLocator.BeginLifetimeScope(web);
        }

        /// <summary>
        /// Method to create a new LifeTime scope used for the lifetime of the container objects within a site
        /// </summary>
        /// <param name="site">The site that define the scope</param>
        /// <returns>A LifeTimeScope</returns>
        public static ILifetimeScope BeginLifetimeScope(SPSite site)
        {
            return serviceLocator.BeginLifetimeScope(site);
        }
    }
}
