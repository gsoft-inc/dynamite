using Autofac;
using GSoft.Dynamite.ServiceLocator.AddOn;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Even through service location should NEVER be done from within the core Dynamite DLL,
    /// this service locator gives you basic service location facilities when you REALLY, REALLY
    /// need it (e.g. you have a serializable/new-able object and you was to access Dynamite logging
    /// utilities from a helper method on that almost-POCO).
    /// </summary>
    internal static class InternalServiceLocator
    {
        /// <summary>
        /// The ambient ServiceLocator will be used, or a fallback default locator will
        /// be provided
        /// </summary>
        private static ISharePointServiceLocator serviceLocator = new AddOnProvidedServiceLocator();

        /// <summary>
        /// Method to create a new child scope of the root application LifeTime scope of the container.
        /// Avoid injecting InstancePerSite, InstancePerWeb or InstancePerRequest-registered objects,
        /// since they will be unreachable.
        /// </summary>
        /// <returns>A LifeTimeScope, direct child of the root application scope</returns>
        public static ILifetimeScope BeginLifetimeScope()
        {
            return serviceLocator.BeginLifetimeScope();
        }
    }
}
