using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Interface for the retrieval of <c>Autofac</c> dependency injection lifetime scopes,
    /// with SharePoint-specific semantics.
    /// Less flexible than <see cref="ISharePointContainerProvider"/>, it is meant to
    /// encourage container usage that depends as little as possible on direct injection 
    /// through the service locator pattern.
    /// </summary>
    public class SharePointServiceLocator : ISharePointServiceLocator
    {
        private ISharePointContainerProvider containerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointServiceLocator"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        public SharePointServiceLocator(string appRootNamespace) : this(appRootNamespace, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointServiceLocator"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher (will be used instead of the AppRootNamespace to
        /// match assembly names in the GAC). The AppRootNamespace still acts as the provided
        /// container's unique key among all the other containers that live in the AppDomain.
        /// </param>
        public SharePointServiceLocator(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            this.containerProvider = new SharePointContainerProvider(appRootNamespace, assemblyFileNameMatcher);
        }

        /// <summary>
        /// Exposes the most-nested currently available lifetime scope.
        /// In an HTTP-request context, will return a shared per-request
        /// scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and InstancePerRequest-registered objects).
        /// Outside an HTTP-request context, will return the root application
        /// container itself (preventing you from injecting InstancePerSite,
        /// InstancePerWeb or InstancePerRequest objects).
        /// Do not dispose this scope, as it will be reused by others.
        /// </summary>
        public ILifetimeScope Current
        {
            get 
            {
                ILifetimeScope currentMostNestedScope = this.containerProvider.Current;

                if (SPContext.Current != null)
                {
                    // If we are in an HTTP request context, don't let people resolve with the
                    // root container (as that is an anti-pattern). Make them resolve from the
                    // most restrictive/nested scope available: the CurrentRequest one.
                    currentMostNestedScope = this.containerProvider.CurrentRequest;
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
        /// In a SPFarm or SPWebApplication feature context, will return a child
        /// container of the root application container (preventing you from injecting
        /// InstancePerSite, InstancePerWeb or InstancePerRequest objects).
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// Prefer usage of this method versus resolving individual dependencies from the 
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
                // We are dealing with a SPWebApplication and SPFarm-scoped feature, so we can
                // only give you a child lifetime that is a direct child of the root container.
                // I.E. this service locator/scope won't be able to resolve any InstancePerSite,
                // InstancePerWeb or InstancePerRequest-registered instances.
                newChildScopeAsNestedAsPossible = this.containerProvider.Current.BeginLifetimeScope();
            }
            else
            {
                // We are dealing with a SPSite or SPWeb-scoped feature.
                // Always return a web scope (even for Site-scoped features - as being in a site-scoped feature means you are in the RootWeb context)
                newChildScopeAsNestedAsPossible = this.containerProvider.EnsureWebScope(currentFeatureWeb).BeginLifetimeScope();
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
        /// <param name="web">The current web from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginWebLifetimeScope(SPWeb web)
        {
            return this.containerProvider.EnsureWebScope(web).BeginLifetimeScope();
        }

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified site
        /// (allowing you to inject InstancePerSite objects).
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// Prefer usage of this method versus resolving manually from the Current property.
        /// </summary>
        /// <param name="site">The current site from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginSiteLifetimeScope(SPSite site)
        {
            return this.containerProvider.EnsureSiteScope(site).BeginLifetimeScope();
        }

        /// <summary>
        /// <c>Autowires</c> the dependencies of a UI control using the current HTTP-request-bound
        /// lifetime scope.
        /// Prefer usage of this method versus resolving individual dependencies from the 
        /// ISharePointServiceLocator.Current property.
        /// </summary>
        /// <param name="target">The UI control which has properties to be injected</param>
        public void InjectProperties(Control target)
        {
            this.Current.InjectProperties(target);
        }

        /// <summary>
        /// <c>Autowires</c> the dependencies of a HttpHandler using the current HTTP-request-bound
        /// lifetime scope.
        /// Prefer usage of this method versus resolving individual dependencies from the 
        /// ISharePointServiceLocator.Current property.
        /// </summary>
        /// <param name="target">The HttpHandler which has properties to be injected</param>
        public void InjectProperties(IHttpHandler target)
        {
            this.Current.InjectProperties(target);
        }
    }
}
