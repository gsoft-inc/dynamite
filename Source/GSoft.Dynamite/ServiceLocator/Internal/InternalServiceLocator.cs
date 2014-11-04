using Autofac;
using GSoft.Dynamite.ServiceLocator.AddOn;

namespace GSoft.Dynamite.ServiceLocator
{
    internal class InternalServiceLocator
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
