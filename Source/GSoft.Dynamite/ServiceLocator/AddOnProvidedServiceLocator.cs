using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Autofac;
using Microsoft.SharePoint;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    // Special service locator which scans the GAC for DLLs that match the 
    // *.ServiceLocator.dll pattern for a ISharePointServiceLocatorAccessor
    // to which it will delegate container provider duties.
    /// </summary>
    public class AddOnProvidedServiceLocator : ISharePointServiceLocator
    {
        private ISharePointServiceLocatorAccessor locatorAccessor;
        private object lockObject = new object();

        public const string KeyServiceLocatorAssemblyName = "ServiceLocatorAssemblyName";

        /// <summary>
        /// Exposes the most-nested currently available lifetime scope.
        /// In an HTTP-request context, will return a shared per-request
        /// scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and IntancePerRequest-registered objects).
        /// Outside an HTTP-request context, will return the root application
        /// container itself (preventing you from injecting InstancePerSite,
        /// InstancePerWeb or InstancePerRequest objects).
        /// Do not dispose this scope, as it will be reused by others.
        /// </summary>
        public ILifetimeScope Current
        {
            get 
            {
                ILifetimeScope currentMostNestedScope = null;

                if (SPContext.Current != null && SPContext.Current.Site != null)
                {
                    this.EnsureServiceLocatorAccessorForCurrentSiteContext(SPContext.Current.Site);

                    currentMostNestedScope = this.locatorAccessor.ServiceLocatorInstance.Current;
                }

                return currentMostNestedScope;
            }
        }

        /// <summary>
        /// Creates a new child lifetime scope that is as nested as possible,
        /// depending on the scope of the specified feature.
        /// In a SPSite or SPWeb-scoped feature context, will return a web-specific
        /// lifetime scope (allowing you to inject InstancePerSite and InstancePerWeb
        /// objects).
        /// In a SPFarm or SPWebApplication feature context, this method will throw
        /// an exception of type <see cref="InvalidOperationException"/>. Dynamite components
        /// must be configured under a specific SPSite's scope.
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// Prefer usage of this method versus resolving indididual dependencies from the 
        /// ISharePointServiceLocator.Current property.
        /// </summary>
        /// <param name="feature">The current feature that is requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginFeatureLifetimeScope(SPFeature feature)
        {
            ILifetimeScope newChildScopeAsNestedAsPossible = null;

            SPSite currentFeatureSite = feature.Parent as SPSite;
            SPWeb currentFeatureWeb = null;

            if (currentFeatureSite == null)
            {
                // this is a Web-scoped feature, not a Site-scoped one
                currentFeatureWeb = feature.Parent as SPWeb;
            }
            else
            {
                // this is a Site-scope feature, use the RootWeb as current
                currentFeatureWeb = currentFeatureSite.RootWeb;
            }

            if (currentFeatureWeb == null)
            {
                // Can't use an AddOnProvidedServiceLocator this way outside a SPSite context (e.g. no SPFarm or SPWebApp scoped feature will work)
                throw new InvalidOperationException("The AddOnProvidedServiceLocator can only work withing a SPSite's context: i.e. only from SPSite or SPWeb-scoped feature event receivers.");
            }
            else
            {
                this.EnsureServiceLocatorAccessorForCurrentSiteContext(currentFeatureWeb.Site);

                // We are dealing with a SPSite or SPWeb-scoped feature.
                // Always return a web scope (even for Site-scoped features - as being in a site-scoped feature means you are in the RootWeb context)
                newChildScopeAsNestedAsPossible = this.locatorAccessor.ServiceLocatorInstance.BeginFeatureLifetimeScope(feature);
            }

            return newChildScopeAsNestedAsPossible;
        }

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified web
        /// (allowing you to inject InstancePerSite and InstancePerWeb objects).
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// Prefer usage of this method versus resolving manually from the Current property.
        /// </summary>
        /// <param name="feature">The current web from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginWebLifetimeScope(SPWeb web)
        {
            ILifetimeScope newWebChildScope = null;

            if (web == null)
            {
                // Can't use an AddOnProvidedServiceLocator this way outside a SPSite context (e.g. no SPFarm or SPWebApp scoped feature will work)
                throw new ArgumentNullException("web");
            }
            else
            {
                this.EnsureServiceLocatorAccessorForCurrentSiteContext(web.Site);

                // We are dealing with a SPSite or SPWeb-scoped feature.
                // Always return a web scope (even for Site-scoped features - as being in a site-scoped feature means you are in the RootWeb context)
                newWebChildScope = this.locatorAccessor.ServiceLocatorInstance.BeginWebLifetimeScope(web);
            }

            return newWebChildScope;
        }

        /// <summary>
        /// Autowires the dependencies of a UI control using the current HTTP-request-bound
        /// lifetime scope.
        /// Prefer usage of this method versus resolving indididual dependencies from the 
        /// ISharePointServiceLocator.Current property.
        /// </summary>
        /// <param name="target">The UI control which has properties to be injected</param>
        public void InjectProperties(Control target)
        {
            this.Current.InjectProperties(target);
        }

        /// <summary>
        /// Autowires the dependencies of a HttpHandler using the current HTTP-request-bound
        /// lifetime scope.
        /// Prefer usage of this method versus resolving indididual dependencies from the 
        /// ISharePointServiceLocator.Current property.
        /// </summary>
        /// <param name="target">The HttpHandler which has properties to be injected</param>
        public void InjectProperties(IHttpHandler target)
        {
            this.Current.InjectProperties(target);
        }

        private void EnsureServiceLocatorAccessorForCurrentSiteContext(SPSite site)
        {
            if (locatorAccessor == null)
            {
                lock (lockObject)
                {
                    if (this.locatorAccessor == null)
                    {
                        try
                        {
                            // 1) Scan the GAC for any DLL matching the *.ServiceLocator.DLL pattern
                            var assemblyScanner = new GacAssemblyLocator();
                            var matchingAssemblies = assemblyScanner.GetAssemblies(new List<string>() { "GAC_MSIL" }, assemblyFileName => assemblyFileName.Contains(".ServiceLocator"));

                            Type accessorType = null;

                            if (matchingAssemblies.Any())
                            {
                                var serviceLocatorAssembly = matchingAssemblies[0];

                                if (matchingAssemblies.Count > 1)
                                {
                                    // 2) If more than one service locator is found, gotta use the contextual SPSite object
                                    //    and extract the preferred service locator setting from its property bag.
                                    if (site != null)
                                    {
                                        using (var rootWeb = site.OpenWeb())
                                        {
                                            string serviceLocatorAssemlyName = rootWeb.Properties[KeyServiceLocatorAssemblyName];

                                            serviceLocatorAssembly = matchingAssemblies.FirstOrDefault(assembly => assembly.FullName.Contains(serviceLocatorAssemlyName));
                                        }
                                    }
                                    else
                                    {
                                        throw new ArgumentNullException("site");
                                    }
                                }

                                if (serviceLocatorAssembly != null)
                                {
                                    // Only one matching assembly, find its accessor class
                                    accessorType = this.FindServiceLocatorAccessorType(serviceLocatorAssembly);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Failed to find an assembly matching the *.ServiceLocator.DLL pattern to provide a service locator.");
                                }
                            }

                            if (accessorType != null)
                            {
                                // 3) Create the accessor instance
                                this.locatorAccessor = (ISharePointServiceLocatorAccessor)Activator.CreateInstance(accessorType);
                            }
                            else
                            {
                                throw new InvalidOperationException("Failed to find implementation of ISharePointServiceLocatorAccessor for AddOnProvidedServiceLocator. Your *.ServiceLocator.DLL assembly should expose its static container through that interface.");
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // Either no assembly in the GAC matches the pattern *.ServiceLocator.DLL pattern, 
                            // or in the matching assembly that was found, no class implements ISharePointServiceLocatorAccessor.
                            // In this case, use our default all-available-Dynamite-modules-only service locator
                            this.locatorAccessor = new FallbackServiceLocator();
                        }

                    }
                }
            }
        }

        private Type FindServiceLocatorAccessorType(System.Reflection.Assembly assembly)
        {
            var accessorInterfaceType = typeof(ISharePointServiceLocatorAccessor);
            return assembly.GetTypes().Where(someType => accessorInterfaceType.IsAssignableFrom(someType) && !someType.IsInterface).FirstOrDefault();
        }
    }
}
