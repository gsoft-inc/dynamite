using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Features.Scanning;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ServiceLocator.Internal;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
	
namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Maintains AppDomain-wide root containers that automatically scan 
    /// the GAC for <c>Autofac</c> dependency injection modules that derive from
    /// the Module <c>Autofac</c> base class.
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
        private static readonly IDictionary<string, IContainer> AppDomainContainersCollection = new Dictionary<string, IContainer>();

        /// <summary>
        /// Returns a service locator instance for the entire application (i.e. throughout 
        /// the current AppDomain). Acts as root Container for all other child lifetime
        /// scopes. Hosts all singletons that are registered as SingleInstance().
        /// Whenever applicable, prefer creating a child lifetime scope instead of resolving 
        /// directly for this root Container instance.
        /// </summary>
        /// <param name="appRootNamespace">The key of the current app</param>
        /// <returns>The container</returns>
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
            if (AppDomainContainersCollection.ContainsKey(appRootNamespace))
            {
                // Return the already-initialized container right away
                return AppDomainContainersCollection[appRootNamespace];
            }

            // Only one container should be registered at a time, to be on the safe side
            lock (ContainersLockObject)
            {
                // Just in case, check again (because the assignment could have happened before we took hold of lock)
                if (AppDomainContainersCollection.ContainsKey(appRootNamespace))
                {
                    return AppDomainContainersCollection[appRootNamespace];
                }

                // We need to filter what we'll be scanning in the GAC (so we pick and choose the DLLs to load Modules 
                // from following the given assemblyFileNameMatch, or we simply try and find all DLLs that match the
                // AppRootNamespace).
                if (assemblyFileNameMatcher == null)
                {
                    assemblyFileNameMatcher = (assemblyFileName) => assemblyFileName.Contains(appRootNamespace);
                }

                // The automatic GAC-scanner factory method will properly configure the injection of Dynamite 
                // utilities and will find and register all injection modules in the GAC DLLs whose file name match.
                AppDomainContainersCollection[appRootNamespace] = ScanGacForAutofacModulesAndCreateContainer(appRootNamespace, assemblyFileNameMatcher);
            }

            return AppDomainContainersCollection[appRootNamespace];
        }

        private static IContainer ScanGacForAutofacModulesAndCreateContainer(string appRootNamespace, Func<string, bool> assemblyFileNameMatchingPredicate)
        {
            using (new SPMonitoredScope("Dynamite - Bootstrapping dependency injection container " + appRootNamespace + " and scanning GAC for Modules."))
            {
                var containerBuilderForDynamiteComponents = new ContainerBuilder();
                var assemblyLocator = new GacAssemblyLocator();

                // Don't just scan the GAC modules, also prepare the Dynamite core utils (by passing the params in ourselves).
                // I.E. each container gets its own DynamiteRegistrationModule components.
                var dynamiteModule = new AutofacDynamiteRegistrationModule(appRootNamespace);
                containerBuilderForDynamiteComponents.RegisterModule(dynamiteModule);

                var matchingAssemblies = assemblyLocator.GetAssemblies(new List<string> { AssemblyFolder }, assemblyFileNameMatchingPredicate);

                // Make sure we exclude all other GSoft.Dynamite DLLs (i.e. ignore other versions deployed to same GAC)
                // so that other AutofacDynamiteRegistrationModule instances don't get registered.
                var filteredMatchingAssemblies = matchingAssemblies.Where(x => !x.FullName.Contains("GSoft.Dynamite,"));

                // Now make sure all Dynamite component modules (i.e. all DLLs that start with GSoft.Dynamite.*) are registered BEFORE
                // any other modules.
                // This ensures that "client" modules will be able to override the Container registrations of GSoft.Dynamite.Components modules.
                var dynamiteComponentModuleAssemblies = filteredMatchingAssemblies.Where(assembly => assembly.FullName.StartsWith("GSoft.Dynamite."));
                var allTheRest = filteredMatchingAssemblies.Where(assembly => !assembly.FullName.StartsWith("GSoft.Dynamite."));

                // 1) Build the base container with only Dynamite-related components
                AutofacBackportScanningUtils.RegisterAssemblyModules(containerBuilderForDynamiteComponents, dynamiteComponentModuleAssemblies.ToArray());
                var container = containerBuilderForDynamiteComponents.Build();

                var logger = container.Resolve<ILogger>();
                string dynamiteAssemblyNameEnumeration = string.Empty;
                dynamiteComponentModuleAssemblies.Cast<Assembly>().ToList().ForEach(a => dynamiteAssemblyNameEnumeration += a.FullName + ", ");
                logger.Info("Dependency injection module registration. The following Dynamite component assemblies were scanned and any Autofac Module within was registered. The order of registrations was: " + dynamiteAssemblyNameEnumeration);

                // 2) Extend the original registrations with any remaining AddOns' registrations
                var containerBuilderForAddOns = new ContainerBuilder();
                AutofacBackportScanningUtils.RegisterAssemblyModules(containerBuilderForAddOns, allTheRest.ToArray());
                containerBuilderForAddOns.Update(container);

                string addOnAssemblyNameEnumeration = string.Empty;
                allTheRest.Cast<Assembly>().ToList().ForEach(a => addOnAssemblyNameEnumeration += a.FullName + ", ");
                logger.Info("Dependency injection module registration. The following Add-On component assemblies (i.e. extensions to the core Dynamite components) were scanned and any Autofac Module within was registered. The order of registrations was: " + addOnAssemblyNameEnumeration);

                // Log the full component registry for easy debugging through ULS
                string componentRegistryAsString = string.Empty;
                var regAndServices = container.ComponentRegistry.Registrations.SelectMany(r => r.Services.OfType<IServiceWithType>(), (r, s) => new { r, s });
                regAndServices.ToList().ForEach(regAndService => componentRegistryAsString += "[" + regAndService.s.ServiceType.FullName + "->" + regAndService.r.Activator.LimitType.FullName + "], ");
                logger.Info("Autofac component registry details: " + componentRegistryAsString);

                return container;
            }
        }
    }
}
