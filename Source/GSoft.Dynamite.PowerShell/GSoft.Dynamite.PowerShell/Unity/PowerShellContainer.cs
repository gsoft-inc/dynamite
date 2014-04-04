using GSoft.Dynamite.DI.Unity;
using Microsoft.Practices.Unity;

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
        /// The lock
        /// </summary>
        private static object SyncRoot = new object();

        /// <summary>
        /// The singleton instance
        /// </summary>
        private static IUnityContainer instance;

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
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            // Bootstrap: the container takes care of registering all of its component modules
                            instance = new UnityRegistrationModuleContainer(
                                new UnityDynamiteUnityIRegistrationModule(AppName, AppName + ".Global"));
                        }
                    }
                }

                return instance;
            }
        }
    }
}
