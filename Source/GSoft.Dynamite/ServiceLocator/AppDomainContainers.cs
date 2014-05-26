// -----------------------------------------------------------------------
// <copyright file="AppContainer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.ServiceLocator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;

    using GSoft.Dynamite.Utils;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System.Reflection;
    using Autofac.Core;
    using Autofac.Builder;
    using Autofac.Core.Lifetime;
    using Autofac.Features.Scanning;
    using GSoft.Dynamite.ServiceLocator.Internal;

    /// <summary>
    /// Maintains AppDomain-wide root containers that automatically scan 
    /// the GAC for Autofac dependency injection modules that derive from
    /// the Module Autofac base class.
    /// </summary>
    /// <remarks>
    /// Only the GAC_MSIL folder of the .NET 3.5 GAC (c:\windows\assembly)
    /// is scanned.
    /// </remarks>
    internal static class AppDomainContainers
    {
        private const string AssemblyFolder = "GAC_MSIL";

        private static readonly object ContainersLockObject = new object();

        /// <summary>
        /// Dictionary of singleton instances that allows us to refer to the 
        /// application root containers
        /// </summary>
        private static readonly IDictionary<string, IContainer> appDomainContainers = new Dictionary<string, IContainer>();

        /// <summary>
        /// Returns a service locator instance for the entire application (i.e. throughout 
        /// the current AppDomain). Acts as root Container for all other child lifetime
        /// scopes. Hosts all singletons that are registered as SingleInstance().
        /// Whenever applicable, prefer creating a child lifetime scope instead of resolving 
        /// directly for this root Container instance.
        /// </summary>
        /// <param name="appRootNamespace">The key of the current app</param>
        public static IContainer CurrentContainer(string appRootNamespace)
        {
            return CurrentContainer(appRootNamespace, null);
        }

        /// <summary>
        /// The current container.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher.
        /// </param>
        /// <returns>
        /// The <see cref="IContainer"/>.
        /// </returns>
        public static IContainer CurrentContainer(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            // Don't bother locking if the instance is already created
            if (appDomainContainers.ContainsKey(appRootNamespace))
            {
                // Return the already-initialized container right away
                return appDomainContainers[appRootNamespace];
            }

            // Only one container should be registered at a time, to be on the safe side
            lock (ContainersLockObject)
            {
                // Just in case, check again (because the assignment could have happened before we took hold of lock)
                if (appDomainContainers.ContainsKey(appRootNamespace))
                {
                    return appDomainContainers[appRootNamespace];
                }

                // We need to filter what we'll be scanning in the GAC (so we pick and choose the DLLs to load Modules 
                // from following the given assemblyFileNameMatch, or we simply try and find all DLLs that match the
                // appRootNamespace).
                if (assemblyFileNameMatcher == null)
                {
                    assemblyFileNameMatcher = (assemblyFileName) => assemblyFileName.Contains(appRootNamespace);
                }

                // The automatic GAC-scanner factory method will properly configure the injection of Dynamite 
                // utilities and will find and register all injection modules in the GAC DLLs whose file name match.
                appDomainContainers[appRootNamespace] = ScanGacForAutofacModulesAndCreateContainer(appRootNamespace, assemblyFileNameMatcher);
            }

            return appDomainContainers[appRootNamespace];
        }

        private static IContainer ScanGacForAutofacModulesAndCreateContainer(string appRootNamespace, Func<string, bool> assemblyFileNameMatchingPredicate)
        {
            using (new SPMonitoredScope("Dynamite - Bootstrapping dependency injection container " + appRootNamespace + " and scanning GAC for Modules."))
            {
                var containerBuilder = new ContainerBuilder();
                var assemblyLocator = new GacAssemblyLocator();

                // Don't just scan the GAC modules, also prepare the Dynamite core utils (by passing the params in ourselves).
                // I.E. each container gets its own DynamiteRegistrationModule components.
                var dynamiteModule = new AutofacDynamiteRegistrationModule(appRootNamespace);
                containerBuilder.RegisterModule(dynamiteModule);

                var matchingAssemblies = assemblyLocator.GetAssemblies(new List<string> { AssemblyFolder }, assemblyFileNameMatchingPredicate);

                // Make sure we exclude all other GSoft.Dynamite DLLs (i.e. ignore other versions deployed to same GAC)
                // so that other AutofacDynamiteRegistrationModule instances don't get registered.
                var filteredMatchingAssemblies = matchingAssemblies.Where(x => !x.FullName.Contains("GSoft.Dynamite,"));

                AutofacBackportScanningUtils.RegisterAssemblyModules(containerBuilder, filteredMatchingAssemblies.ToArray());

                return containerBuilder.Build();
            }
        }
    }
}
