using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Unity;
using Microsoft.Practices.Unity;

namespace GSoft.Dynamite.PowerShell.Unity
{
    public static class PowerShellContainer
    {
        /// <summary>
        /// The application name
        /// </summary>
        private const string AppName = "G.Dynamite.PowerShell";

        /// <summary>
        /// The lock
        /// </summary>
        private static object SyncRoot = new object();

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
                    lock (SyncRoot)
                    {
                        if (instance == null)
                        {
                            // Bootstrap: the container takes care of registering all of its component modules
                            instance = new RegistrationModuleContainer(
                                new GRegistrationModule(AppName, AppName + ".Global"));
                        }
                    }
                }

                return instance;
            }
        }
    }
}
