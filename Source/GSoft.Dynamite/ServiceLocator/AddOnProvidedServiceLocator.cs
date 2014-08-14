using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Autofac;
using Microsoft.SharePoint;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint.Administration;
using GSoft.Dynamite.Logging;
using System.Reflection;
using System.Globalization;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Special service locator which scans the GAC for DLLs that match the 
    /// *.ServiceLocator.dll pattern for a ISharePointServiceLocatorAccessor
    /// to which it will delegate container provider duties.
    /// 
    /// Thanks to these ServiceLocator-bootstrapping mechanics, you can build
    /// reusable "framework" SharePoint components that can have their inner
    /// implementations overrided by AddOns' registration module (since the
    /// AddOn's ServiceLocator is responsible for determining the final set
    /// of all registration modules that will be loaded).
    /// </summary>
    public class AddOnProvidedServiceLocator : ISharePointServiceLocator
    {
        private ISharePointServiceLocatorAccessor locatorAccessor;
        private object lockObject = new object();

        public const string KeyServiceLocatorAssemblyName = "ServiceLocatorAssemblyName";

        /// <summary>
        /// Exposes the most-nested currently available lifetime scope.
        /// 
        /// In an HTTP-request context, will return a shared per-request
        /// scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and IntancePerRequest-registered objects). Be sure to enable Dynamite's
        /// feature HttpModule feature: "GSoft.Dynamite.SP_Web Config Modifications" so
        /// that InstancePerRequest-scoped objects get properly disposed at the end of
        /// every HttpRequest.
        /// 
        /// Outside an HTTP-request context, will return the root application
        /// container itself (preventing you from injecting InstancePerSite,
        /// InstancePerWeb or InstancePerRequest objects).  If more than two DLLs exist in GAC that match the 
        /// *.ServiceLocator.DLL filename pattern, and access to this member is responsible 
        /// for DI bootstrapping at application startup, due to lack of context it will be impossible 
        /// to disambiguate between the available containers. Use BeginLifetimeScope(SPFeature) or 
        /// BeginLifetimeScope(SPWeb) or BeginLifetimeScope(SPSite) or BeginLifetimeScope(SPWebApplication) 
        /// instead when outside an HTTP-request context (e.g. Cmdlets, FeatureActivated, etc.).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPWeb-SPSite-SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Do not dispose this scope, as it will be reused by others. Prefer using
        /// BeginLifetimeScope() within a using block to this method to ensure all
        /// IDisposable objects you inject get properly disposed.
        /// </summary>
        [Obsolete("Prefer usage of BeginLifetimeScope() from a using block to ensure proper disposal of all IDisposable objects you injected.")]
        public ILifetimeScope Current
        {
            get 
            {               
                if (SPContext.Current != null && SPContext.Current.Web != null)
                {
                    this.EnsureServiceLocatorAccessorForCurrentContext(SPContext.Current.Web);
                }
                else
                {
                    // Empty context (not an HttpRequest within a SharePoint site collection)
                    this.EnsureServiceLocatorAccessorForCurrentContext();
                }

                return this.locatorAccessor.ServiceLocatorInstance.Current;
            }
        }

        /// <summary>
        /// Creates a new child lifetime scope - a child to the most-nested currently 
        /// available lifetime scope.
        /// 
        /// In an HTTP-request context, will return a child scope to the shared 
        /// per-request scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and InstancePerRequest-registered objects). Be sure to enable Dynamite's
        /// feature HttpModule feature: "GSoft.Dynamite.SP_Web Config Modifications" so
        /// that InstancePerRequest-scoped objects get properly disposed at the end of
        /// every HttpRequest.
        /// 
        /// Outside an HTTP-request context, will return the a child of the root application
        /// container itself (preventing you from injecting InstancePerSite, InstancePerWeb 
        /// or InstancePerRequest objects). If more than two DLLs exist in GAC that match the 
        /// *.ServiceLocator.DLL filename pattern, and access to this member is responsible 
        /// for DI bootstrapping at application startup, due to lack of context it will be impossible 
        /// to disambiguate between the available containers. Use BeginLifetimeScope(SPFeature) or 
        /// BeginLifetimeScope(SPWeb) or BeginLifetimeScope(SPSite) or BeginLifetimeScope(SPWebApplication) 
        /// instead when outside an HTTP-request context (e.g. Cmdlets, FeatureActivated, etc.).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPWeb-SPSite-SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return this.Current.BeginLifetimeScope();
        }

        /// <summary>
        /// Creates a new child lifetime scope that is as nested as possible,
        /// depending on the scope of the specified feature.
        /// 
        /// In a SPSite or SPWeb-scoped feature context, will return a web-specific
        /// lifetime scope (allowing you to inject InstancePerSite and InstancePerWeb
        /// objects - InstancePerRequest scoped objects will be inaccessible).
        /// 
        /// In a SPFarm or SPWebApplication feature context, will return a child
        /// container of the root application container (preventing you from injecting
        /// InstancePerSite, InstancePerWeb or InstancePerRequest objects).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPWeb-SPSite-SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="feature">The current feature context from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPFeature feature)
        {
            SPWeb currentFeatureWeb = feature.Parent as SPWeb;
            SPSite currentFeatureSite = feature.Parent as SPSite;
            SPWebApplication currentFeatureWebApp = feature.Parent as SPWebApplication;
            SPFarm currentFeatureFarm = feature.Parent as SPFarm;

            if (currentFeatureWeb != null)
            {
                this.EnsureServiceLocatorAccessorForCurrentContext(currentFeatureWeb);
            }
            else if (currentFeatureSite != null)
            {
                this.EnsureServiceLocatorAccessorForCurrentContext(currentFeatureSite);
            }
            else if (currentFeatureWebApp != null)
            {
                this.EnsureServiceLocatorAccessorForCurrentContext(currentFeatureWebApp);
            }
            else if (currentFeatureFarm != null)
            {
                this.EnsureServiceLocatorAccessorForCurrentContext(currentFeatureFarm);
            }
            else
            {
                this.EnsureServiceLocatorAccessorForCurrentContext();
            }

            return this.locatorAccessor.ServiceLocatorInstance.BeginLifetimeScope(feature);;
        }

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified web
        /// (allowing you to inject InstancePerSite and InstancePerWeb objects - InstancePerRequest
        /// scoped objects will be inaccessible).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPWeb-SPSite-SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="web">The current web from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPWeb web)
        {
            this.EnsureServiceLocatorAccessorForCurrentContext(web);
            return this.locatorAccessor.ServiceLocatorInstance.BeginLifetimeScope(web);
        }

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified site collection
        /// (allowing you to inject InstancePerSite objects - InstancePerWeb and InstancePerRequest
        /// scoped objects will be inaccessible).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPSite-SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="site">The current site collection from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPSite site)
        {
            this.EnsureServiceLocatorAccessorForCurrentContext(site);
            return this.locatorAccessor.ServiceLocatorInstance.BeginLifetimeScope(site);
        }

        /// <summary>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in one of the SPPersistedObject's property bags in the SPWebApp-SPFarm 
        /// hierarchy to indicate which ServiceLocator should be used in your context. If
        /// the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="webApplication">The current context's web application</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPWebApplication webApplication)
        {
            this.EnsureServiceLocatorAccessorForCurrentContext(webApplication);
            return this.locatorAccessor.ServiceLocatorInstance.BeginLifetimeScope(webApplication);
        }

        /// <summary>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// 
        /// If more than 1 assembly matches the *.ServiceLocator.DLL pattern in the GAC,
        /// store your preferred ServiceLocator assembly name (with key: 'ServiceLocatorAssemblyName') 
        /// in the SPFarm property bag to indicate which ServiceLocator should be used in your context. 
        /// If the disambiguator setting cannot be found in any of the property bags in the
        /// hierarchy, an error will be logged to ULS and the FallbackServiceLocator will be used
        /// (preventing your AddOn registration modules from being loaded).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="site">The current context's farm</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPFarm farm)
        {
            this.EnsureServiceLocatorAccessorForCurrentContext(farm);
            return this.locatorAccessor.ServiceLocatorInstance.BeginLifetimeScope(farm);
        }

        private void EnsureServiceLocatorAccessorForCurrentContext()
        {
            // Empty context, this is ok until we find more than one *.ServiceLocator.DLL
            // assemblies in the GAC. At that point, without a context to look in for
            // property bags and the ServiceLocatorAssemblyName setting, we won't be
            // able to disambiguate between the many service locators.
            this.EnsureServiceLocatorAccessor(null, null, null, null);
        }

        private void EnsureServiceLocatorAccessorForCurrentContext(SPWeb web)
        {
            this.EnsureServiceLocatorAccessor(web, web.Site, web.Site.WebApplication, web.Site.WebApplication.Farm);
        }

        private void EnsureServiceLocatorAccessorForCurrentContext(SPSite site)
        {
            this.EnsureServiceLocatorAccessor(null, site, site.WebApplication, site.WebApplication.Farm);
        }

        private void EnsureServiceLocatorAccessorForCurrentContext(SPWebApplication webApplication)
        {
            this.EnsureServiceLocatorAccessor(null, null, webApplication, webApplication.Farm);
        }

        private void EnsureServiceLocatorAccessorForCurrentContext(SPFarm farm)
        {
            this.EnsureServiceLocatorAccessor(null, null, null, farm);
        }

        /// <summary>
        /// Triggers ServiceLocator bootstrapping (scans the GAC for assemblies with a name
        /// that matches *.ServiceLocator.DLL, by convention).
        /// </summary>
        /// <param name="web">The context's SPWeb. Keep null if none available.</param>
        /// <param name="site">The context's SPSite. Keep null if none available.</param>
        /// <param name="webApplication">The context's SPWebApplication. Keep null if none available.</param>
        /// <param name="farm">The context's SPFarm. Keep null if none available.</param>
        private void EnsureServiceLocatorAccessor(SPWeb web, SPSite site, SPWebApplication webApplication, SPFarm farm)
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

                            Assembly serviceLocatorAssembly = null;
                            Type accessorType = null;

                            if (matchingAssemblies.Any())
                            {

                                if (matchingAssemblies.Count > 1)
                                {
                                    // 2) If more than one service locator is found, we must disambiguate. We have to use the 
                                    //    contextual SPWeb, SPSite, SPWebApp or SPFarm objects and extract the preferred service 
                                    //    locator assembly name setting from their property bag.
                                    //    The SPWeb's property bag is inspected first, if available, then the SPSite's RootWeb property
                                    //    bag, then the SPWebApp's, then the SPFarm's property bag as a last resort.
                                    string contextObjectWhereDiscriminatorWasFound;
                                    string serviceLocatorAssemblyNameDiscriminator = this.FindServiceLocatorAccessorTypeNameFromMostSpecificPropertyBag(web, site, webApplication, farm, out contextObjectWhereDiscriminatorWasFound);

                                    string allServiceLocatorAssemblyNames = string.Join(";", matchingAssemblies.Select(locatorAssembly => locatorAssembly.FullName).ToArray());
                                    string basicDisambiguationErrorMessage = string.Format(
                                        CultureInfo.InvariantCulture,
                                        "Failed to disambiguate between all DLLs in the GAC that match the *.ServiceLocator.DLL filename pattern. All matching assemblies in GAC: {0}.",
                                        allServiceLocatorAssemblyNames);

                                    if (!string.IsNullOrEmpty(serviceLocatorAssemblyNameDiscriminator))
                                    {
                                        // We found a ServiceLocator assembly name in one of the context's Property Bags.
                                        serviceLocatorAssembly = matchingAssemblies.FirstOrDefault(assembly => assembly.FullName.Contains(serviceLocatorAssemblyNameDiscriminator));

                                        if (serviceLocatorAssembly == null)
                                        {
                                            throw new InvalidOperationException(basicDisambiguationErrorMessage +
                                                " The discriminator found in one of the context's Property Bags (value=" + serviceLocatorAssemblyNameDiscriminator +
                                                ", property bag location=" + contextObjectWhereDiscriminatorWasFound + ") did not match either of the " + 
                                                matchingAssemblies.Count + " ServiceLocator DLLs available in GAC. The discriminator value should match one of the DLLs so that we can determine which to use.");
                                        }
                                    }
                                    else
                                    {
                                        // We failed to find a disambiguator setting in all of the context's Property Bags
                                        throw new InvalidOperationException(basicDisambiguationErrorMessage +
                                            " You cannot begin injection from the root application container if more that one ServiceLocator assembly exists in the GAC." +
                                            " You must begin injection with one of the following methods on your ISharePointServiceLocator: BeginLifetimeScope(SPFeature) or" +
                                            " BeginLifetimeScope(SPWeb) or BeginLifetimeScope(SPSite) or BeginLifetimeScope(SPWebApplication) or BeginLifetimeScope(SPFarm)," +
                                            " depending on your context. IMPORTANT: The property bags on the context' SPWeb, SPSite, SPWebApplication and SPFarm will be inspected" +
                                            " (in that order) to find a value for the key '" + KeyServiceLocatorAssemblyName + "'. This discriminator value will indicate to Dynamite's" +
                                            " AddOnProvidedServiceLocator which concrete add-on's ServiceLocator DLL to use in the current context.");
                                    }
                                }
                                else
                                {
                                    // Only one ServiceLocator DLL found in GAC. There is no ambiguity: use this locator.
                                    serviceLocatorAssembly = matchingAssemblies[0];
                                }

                                if (serviceLocatorAssembly != null)
                                {
                                    // At this point we figured out the right matching assembly: find its accessor class within its types
                                    accessorType = this.FindServiceLocatorAccessorType(serviceLocatorAssembly);
                                }
                            }
                            else
                            {
                                // Not even one DLL in GAC matches the *.ServiceLocator.DLL pattern
                                throw new InvalidOperationException("Failed to find any assembly in the GAC that matches the *.ServiceLocator.DLL pattern.");
                            }

                            if (accessorType != null)
                            {
                                // 3) Create the accessor instance
                                this.locatorAccessor = (ISharePointServiceLocatorAccessor)Activator.CreateInstance(accessorType);
                            }
                            else
                            {
                                throw new InvalidOperationException("Failed to find implementation of ISharePointServiceLocatorAccessor for AddOnProvidedServiceLocator. Your ServiceLocator assembly (" + serviceLocatorAssembly.FullName + ") should expose its static container through that interface.");
                            }
                        }
                        catch (InvalidOperationException exception)
                        {
                            var logger = new TraceLogger("GSoft.Dynamite", "GSoft.Dynamite", false);
                            logger.Error(
                                "AddOnProvidedServiceLocator Initialization Error - An error occured while trying to find a DLL matching the pattern *ServiceLocator.dll in the GAC. The FallbackServiceLocator will be used instead as a last resort (no AddOn registration module will be registered). Exception: {0}", 
                                exception.ToString());

                            // Either no assembly in the GAC matches the pattern *.ServiceLocator.DLL pattern, 
                            // or in the matching assembly that was found, no class implements ISharePointServiceLocatorAccessor.
                            // In this case, use our default all-available-Dynamite-modules-only service locator
                            this.locatorAccessor = new FallbackServiceLocator();
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Inspects the property bags of all SPPersistedObjects in the 
        /// context, from SPWeb to SPSite to SPWebApplication to SPFarm.
        /// </summary>
        /// <param name="web">The context's SPWeb. Keep null if none available.</param>
        /// <param name="site">The context's SPSite. Keep null if none available.</param>
        /// <param name="webApplication">The context's SPWebApplication. Keep null if none available.</param>
        /// <param name="farm">The context's SPFarm. Keep null if none available.</param>
        /// <param name="locationWhereDiscriminatorWasFound">A out-param string that returns the identity of the SPPersistedObject where the disambiguator setting was found</param>
        /// <returns>The ServiceLocatorAssemblyName disambiguator settings, if found in one of the context objects' property bags</returns>
        private string FindServiceLocatorAccessorTypeNameFromMostSpecificPropertyBag(SPWeb web, SPSite site, SPWebApplication webApplication, SPFarm farm, out string locationWhereDiscriminatorWasFound)
        {
            if (web != null && web.Properties.ContainsKey(KeyServiceLocatorAssemblyName))
            {
                locationWhereDiscriminatorWasFound = "SPWeb @ " + web.Url;
                return web.Properties[KeyServiceLocatorAssemblyName];
            }
            else if (site != null && site.RootWeb.Properties.ContainsKey(KeyServiceLocatorAssemblyName))
            {
                locationWhereDiscriminatorWasFound = "SPSite.RootWeb @ " + site.RootWeb.Url;
                return site.RootWeb.Properties[KeyServiceLocatorAssemblyName];
            }
            else if (webApplication != null && webApplication.Properties.ContainsKey(KeyServiceLocatorAssemblyName))
            {
                locationWhereDiscriminatorWasFound = "SPWebApplication @ " + webApplication.DisplayName;
                return (string)webApplication.Properties[KeyServiceLocatorAssemblyName];
            }
            else if (farm != null && farm.Properties.ContainsKey(KeyServiceLocatorAssemblyName))
            {
                locationWhereDiscriminatorWasFound = "SPFarm @ " + farm.DisplayName;
                return (string)farm.Properties[KeyServiceLocatorAssemblyName];
            }
            else
            {
                locationWhereDiscriminatorWasFound = "Not found!!!";
                return string.Empty;
            }
        }

        /// <summary>
        /// Loops through all Types in an assembly to find one that implements
        /// the <see cref="ISharePointServiceLocatorAccessor"/> interface, so
        /// that it can be used to access the preferred AddOn's ServiceLocator.
        /// </summary>
        /// <param name="assembly">The assembly to scan</param>
        /// <returns>The AddOn's service locator accessor type</returns>
        private Type FindServiceLocatorAccessorType(Assembly assembly)
        {
            var accessorInterfaceType = typeof(ISharePointServiceLocatorAccessor);
            return assembly.GetTypes().Where(someType => accessorInterfaceType.IsAssignableFrom(someType) && !someType.IsInterface).FirstOrDefault();
        }
    }
}
