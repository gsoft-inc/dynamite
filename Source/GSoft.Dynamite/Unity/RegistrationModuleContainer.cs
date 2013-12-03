using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Unity
{
    /// <summary>
    /// Modularized Unity container
    /// </summary>
    public class RegistrationModuleContainer : UnityContainer
    {
        /// <summary>
        /// Creates an empty registration module container
        /// </summary>
        public RegistrationModuleContainer() : base()
        {
        }

        /// <summary>
        /// Creates a container and immediately registers the type bindings
        /// of the input modules
        /// </summary>
        /// <param name="modules">Type binding modules for the application</param>
        public RegistrationModuleContainer(params IRegistrationModule[] modules)
            : this()
        {
            foreach (IRegistrationModule module in modules)
            {
                module.Register(this);
            }            
        }

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <remarks>
        /// This is a convenience method meant to save us the hassle of always depending on the
        /// usual IUnityContain.Resolve extension method from Microsoft.Practices.Unity, which
        /// forces us to always refer to that namespace.
        /// </remarks>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <returns>The implementation of the type specified</returns>
        public T Resolve<T>()
        {
            return UnityContainerExtensions.Resolve<T>(this);
        }

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <param name="overrides">Resolver overrides</param>
        /// <returns>The implementation of the type specified</returns>
        public T Resolve<T>(params ResolverOverride[] overrides)
        {
            return UnityContainerExtensions.Resolve<T>(this, overrides);
        }

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <param name="name">The name of the registration</param>
        /// <returns>The implementation of the type specified</returns>
        public T Resolve<T>(string name)
        {
            return UnityContainerExtensions.Resolve<T>(this, name);
        }
    }
}
