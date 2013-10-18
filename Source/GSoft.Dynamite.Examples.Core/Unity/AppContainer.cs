using GSoft.Dynamite.Unity;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.Examples.Unity
{
    /// <summary>
    /// Dependency injection container
    /// </summary>
    public static class AppContainer
    {
        /// <summary>
        /// The application name
        /// </summary>
        private const string AppName = "GSoft.Dynamite.Examples";

        /// <summary>
        /// The singleton instance
        /// </summary>
        private static IUnityContainer instance = null;

        /// <summary>
        /// Dependency injection container instance
        /// </summary>
        public static IUnityContainer Current
        {
            get
            {
                if (instance == null)
                {
                    // The injection should be bootstrapped only once
                    lock (typeof(AppContainer))
                    {
                        if (instance == null)
                        {
                            // Bootstrap: the container takes care of registering all of its component modules
                            instance = new RegistrationModuleContainer(
                                new GRegistrationModule(AppName, AppName + ".Global"),
                                new WallRegistrationModule());
                        }
                    }
                }

                return instance;
            }
        }
    }
}
