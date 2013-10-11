using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Sharepoint.Unity
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
    }
}
