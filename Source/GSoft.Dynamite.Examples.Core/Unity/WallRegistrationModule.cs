using GSoft.Dynamite.Examples.Repositories;
using GSoft.Dynamite.Unity;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Examples.Unity
{
    /// <summary>
    /// Type bindings for Wall components
    /// </summary>
    public class WallRegistrationModule : IRegistrationModule
    {
        /// <summary>
        /// Registers the Wall type bindings
        /// </summary>
        /// <param name="container">The dependency injection container</param>
        public void Register(IUnityContainer container)
        {
            // Repositories
            container.RegisterType<IWallPostRepository, WallPostRepository>();
            container.RegisterType<IWallReplyRepository, WallReplyRepository>();
        }
    }
}
