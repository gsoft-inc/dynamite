// -----------------------------------------------------------------------
// <copyright file="AppContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Autofac;
    using GSoft.Dynamite.Utils;
    using System.Reflection;
    using Microsoft.SharePoint.Utilities;

    /// <summary>
    /// Reference to AppDomain-wide root container that automatically scans 
    /// the GAC for Autofac dependency injection modules that derive from
    /// the Module base class.
    /// </summary>
    /// <remarks>
    /// Only the GAC_MSIL folder of the .NET 3.5 GAC (c:\windows\assembly)
    /// is scanned.
    /// </remarks>
    /// <example>
    /// Sharing all instances registered with InstancePerLiftetimeScope throughout
    /// the current request (requires the <see cref="DynamiteAutofacHttpModule"/>):
    /// <![CDATA[ 
    /// var myPerRequestCache = AppContainer.CurrentRequest.Resolve<ISomePerRequestCache>();
    /// ]]>
    /// 
    /// Similarly, object sharing scoped to a site collection:
    /// <![CDATA[ 
    /// var userService = AppContainer.CurrentSite.Resolve<IUserService>();
    /// ]]>
    /// 
    /// Using Dynamite utilities from a feature event receiver:
    /// <![CDATA[ 
    /// using (var childScope = AppContainer.Current.BeginLifetimeScope())
    /// {
    ///     var logger = childScope.Resolve<ILogger>();
    ///     var taxonomyService = childScope.Resolve<ITaxonomyService>();
    ///     
    ///     var currentSite = properties.Feature.Parent as SPSite;
    ///     taxonomyService.GetTermForId(currentSite, Guid.NewsGuid());
    ///     logger.Info("Tough luck!");
    /// }
    /// ]]>
    /// </example>
    public static class AppContainer
    {
        private const string AssemblyFolder = "GAC_MSIL";

        private static readonly object LockObject = new object();

        /// <summary>
        /// The singleton instance that allows us to refer to the 
        /// application root container in a static fashion
        /// </summary>
        private static volatile IContainer instance = null;

        /// <summary>
        /// Returns a service locator instance for the entire application (i.e. throughout 
        /// the current AppDomain). Acts as root Container for all other child lifetime
        /// scopes. Hosts all singletons that are registered as SingleInstance().
        /// Use CurrentRequest, CurrentSite or CurrentWeb lifetime scope instead when in HTTP 
        /// request context (see remarks).
        /// Whenever applicable, prefer creating a child lifetime scope instead of resolving 
        /// directly for this root Container instance.
        /// </summary>
        /// <remarks>
        /// - The first access to this singleton after a IIS app pool recycle will cause
        ///   the assembly scanning and dependency injection bootstrapping.  
        /// - Requires the <see cref="DynamiteAutofacHttpModule"/>: Prefer using the 
        ///   CurrentRequest property instead of Current, especially when resolving a tree of 
        ///   dependencies that involves IDisposable objects. The CurrentRequest lifetime scope 
        ///   serves as a per-HTTP-request deletion boundary for such resources and allows 
        ///   object sharing throughout the entire request. 
        /// - Use the .InstancePerLiftetimeScope() or .InstancePerRequest() lifetime
        ///   registrations along with AppContainer.Current.BeginLifetimeScope() instead
        ///   of calling Resolve directly on this property.
        /// </remarks>
        /// <param name="appName">The key of the current app</param>
        public static IContainer Current(string appName)
        {
            // Don't bother locking if the instance is already created
            if (instance != null)
            {
                return instance;
            }

            // The injection should be bootstrapped only once
            lock (LockObject)
            {
                // Just in case, check again (because the assignment could have happened before we took hold of lock)
                if (instance != null)
                {
                    return instance;
                }

                // Dynamite registration module expects these parameters:
                Func<string, bool> assemblyMatcher = (assemblyFileName) => assemblyFileName.Contains(appName);
                //var logCategoryName = appName;
                //var defaultResourceFileNames = new string[] 
                //    { 
                //        appName, 
                //        appName + ".News", 
                //        appName + ".ConfigurationValues", 
                //        appName + ".ReusableContent", 
                //        appName + ".Navigation", 
                //        appName + ".ProvinceToBU", 
                //        appName + ".UserReports"
                //    };

                // The automatic GAC-scanner factory method will properly configure the injection of Dynamite 
                // utilities and will find and register all injection modules in the GAC DLLs that match 
                // "IFC.IntactNet" and "Core" in their namespace.
                instance = ScanGacForAutofacModulesAndCreateContainer(assemblyMatcher, appName);
            }

            return instance;
        }

        private static IContainer ScanGacForAutofacModulesAndCreateContainer(Func<string, bool> assemblyNameMatchingPredicate, string appName)
        {
            using (new SPMonitoredScope("Dynamite - Bootstrapping dependency injection container and scanning GAC for Modules."))
            {
                var containerBuilder = new ContainerBuilder();
                var assemblyLocator = new GacAssemblyLocator();

                // Don't just scan the GAC modules, also prepare the Dynamite core utils (by passing the params in ourselves)
                var dynamiteModule = new AutofacDynamiteRegistrationModule(appName);
                containerBuilder.RegisterModule(dynamiteModule);

                var matchingAssemblies = assemblyLocator.GetAssemblies(new List<string> { AssemblyFolder }, assemblyNameMatchingPredicate);

                // Make sure we exclude all other GSoft.Dynamite DLLs (i.e. ignore other versions deployed to same GAC)
                // so that other AutofacDynamiteRegistrationModule instances don't get registered.
                var filteredMatchingAssemblies = matchingAssemblies.Where(
                    x => x.FullName != Assembly.GetExecutingAssembly().FullName && !x.FullName.Contains("GSoft.Dynamite,"));

                containerBuilder.RegisterAssemblyModules(filteredMatchingAssemblies.ToArray());

                return containerBuilder.Build();
            }
        }
    }
}
