using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Sharepoint.Unity
{
    /// <summary>
    /// Interface for type binding modules
    /// </summary>
    public interface IRegistrationModule
    {
        /// <summary>
        /// Registers the module's type bindings on the container
        /// </summary>
        /// <param name="container">The Unity dependency injection container</param>
        void Register(IUnityContainer container);
    }
}
