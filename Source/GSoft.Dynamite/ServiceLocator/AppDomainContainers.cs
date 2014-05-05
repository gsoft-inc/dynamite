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

    /// <summary>
    /// Maintains AppDomain-wide root containers that automatically scan 
    /// the GAC for Autofac dependency injection modules that derive from
    /// the Module Autofac base class.
    /// </summary>
    /// <remarks>
    /// Only the GAC_MSIL folder of the .NET 3.5 GAC (c:\windows\assembly)
    /// is scanned.
    /// </remarks>
    /// <example>
    /// How to share all instances registered with InstancePerLiftetimeScope throughout
    /// the current request (requires the <see cref="DynamiteAutofacHttpModule"/>):
    /// <![CDATA[ 
    /// var myPerRequestCache = AppContainers.CurrentRequest.Resolve<ISomePerRequestCache>();
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
    public static class AppDomainContainers
    {
        private const string AssemblyFolder = "GAC_MSIL";

        private static readonly object ContainersLockObject = new object();

        /// <summary>
        /// Dictionary of singleton instances that allows us to refer to the 
        /// application root containers
        /// </summary>
        private static readonly IDictionary<string, IContainer> appDomainContainers = new Dictionary<string, IContainer>();

        private static readonly object ChildScopesLockObject = new object();
        
        /// <summary>
        /// Shared dictionary of container child scopes, sandboxed child containers that are meant to live as
        /// long as their parent root container
        /// </summary>
        private static readonly IDictionary<string, ILifetimeScope> uniqueChildScopes = new Dictionary<string, ILifetimeScope>();

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
                // utilities and will find and register all injection modules in the GAC DLLs that match 
                // "IFC.IntactNet" and "Core" in their namespace.
                appDomainContainers[appRootNamespace] = ScanGacForAutofacModulesAndCreateContainer(appRootNamespace, assemblyFileNameMatcher);
            }

            return appDomainContainers[appRootNamespace];
        }

        /// <summary>
        /// The current site scope.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        public static ILifetimeScope CurrentSiteScope(string appRootNamespace)
        {
            return CurrentSiteScope(appRootNamespace, null);
        }

        /// <summary>
        /// The current site scope.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        public static ILifetimeScope CurrentSiteScope(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            ThrowExceptionIfNotSPContext(appRootNamespace);

            var currentSiteKey = SPContext.Current.Site.ID.ToString();
            return EnsureUndisposableScopeForTagInContainer(appRootNamespace, assemblyFileNameMatcher, currentSiteKey);
        }

        /// <summary>
        /// The current web scope.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        public static ILifetimeScope CurrentWebScope(string appRootNamespace)
        {
            return CurrentWebScope(appRootNamespace, null);
        }

        /// <summary>
        /// The current web scope.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        public static ILifetimeScope CurrentWebScope(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            ThrowExceptionIfNotSPContext(appRootNamespace);

            var currentWebKey = SPContext.Current.Web.ID.ToString();
            return EnsureUndisposableScopeForTagInContainer(appRootNamespace, assemblyFileNameMatcher, currentWebKey);
        }        

        private static void ThrowExceptionIfNotSPContext(string appRootNamespace)
        {
            if (SPContext.Current == null)
            {
                throw new InvalidOperationException(
                    "Can't access current site lifetime scope for container " + appRootNamespace + " because not in a SharePoint web request context. "
                    + "Instead, to force a sharing boundary for classes registered as InstancePerLifetimeScope, create your own lifetime scope with using(var childScope = YourRootContainer.Current.BeginLifetimeScope()) {}.");
            }
        }

        private static ILifetimeScope EnsureUndisposableScopeForTagInContainer(string containerKey, Func<string, bool> assemblyFileNameMatcher, string scopeTag)
        {
            ILifetimeScope ensuredScope = null;

            var container = CurrentContainer(containerKey, assemblyFileNameMatcher);
            var fullKey = containerKey + "-" + scopeTag;

            // Don't bother locking if the instance is already created
            if (uniqueChildScopes.ContainsKey(fullKey))
            {
                // Return the already-initialized container right away
                ensuredScope = uniqueChildScopes[fullKey];
            }
            else
            {
                // Only one scope should be registered at a time, to be on the safe side
                lock (ChildScopesLockObject)
                {
                    // Just in case, check again (because the assignment could have happened before we took hold of lock)
                    if (uniqueChildScopes.ContainsKey(fullKey))
                    {
                        ensuredScope = uniqueChildScopes[fullKey];
                    }
                    else
                    {
                        // This scope will never be disposed, i.e. it will life as long as the parent
                        // container, provided no one calls Dispose on it.
                        // The scope only meant to sandbox InstancePerLifetimeScope-registered objects
                        // to be shared only within a boundary uniquely identified by the key.
                        ensuredScope = container.BeginLifetimeScope(fullKey);
                        uniqueChildScopes[fullKey] = ensuredScope;
                    }
                }
            }

            return ensuredScope;
        }

        private static IContainer ScanGacForAutofacModulesAndCreateContainer(string appRootNamespace, Func<string, bool> assemblyFileNameMatchingPredicate)
        {
            using (new SPMonitoredScope("Dynamite - Bootstrapping dependency injection container and scanning GAC for Modules."))
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

                containerBuilder.RegisterAssemblyModules(filteredMatchingAssemblies.ToArray());

                return containerBuilder.Build();
            }
        }
    }
}
