using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Autofac;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// <para>
    /// Implementation for the retrieval of <c>Autofac</c> dependency injection lifetime scopes,
    /// with SharePoint-specific semantics.
    /// </para>
    /// <para>
    /// Less flexible than <see cref="SharePointContainerProvider"/>, it is meant to
    /// encourage container usage that depends as little as possible on direct injection 
    /// through the service locator pattern.
    /// </para>
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
        /// <para>
        /// Exposes the most-nested currently available lifetime scope.
        /// </para>
        /// <para>
        /// In an HTTP-request context, will return a shared per-request
        /// scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and InstancePerRequest-registered objects). Be sure to enable Dynamite's
        /// feature HttpModule feature: "GSoft.Dynamite.SP_Web Config Modifications" so
        /// that InstancePerRequest-scoped objects get properly disposed at the end of
        /// every HttpRequest.
        /// </para>
        /// <para>
        /// Outside an HTTP-request context, will return the root application
        /// container itself (preventing you from injecting InstancePerSite,
        /// InstancePerWeb or InstancePerRequest objects).
        /// </para>
        /// <para>
        /// Do not dispose this scope, as it will be reused by others. Prefer using
        /// BeginLifetimeScope() within a using block to this method to ensure all
        /// IDisposable objects you inject get properly disposed.
        /// </para>
        /// </summary>
        [Obsolete("Prefer usage of BeginLifetimeScope() from a using block to ensure proper disposal of all IDisposable objects you injected.")]
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
        /// <para>
        /// Creates a new child lifetime scope - a child to the most-nested currently 
        /// available lifetime scope.
        /// </para>
        /// <para>
        /// In an HTTP-request context, will return a child scope to the shared 
        /// per-request scope (allowing you to inject InstancePerSite, InstancePerWeb
        /// and InstancePerRequest-registered objects). Be sure to enable Dynamite's
        /// feature HttpModule feature: "GSoft.Dynamite.SP_Web Config Modifications" so
        /// that InstancePerRequest-scoped objects get properly disposed at the end of
        /// every HttpRequest.
        /// </para>
        /// <para>
        /// Outside an HTTP-request context, will return the a child of the root application
        /// container itself (preventing you from injecting InstancePerSite, InstancePerWeb 
        /// or InstancePerRequest objects).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return this.Current.BeginLifetimeScope();
        }

        /// <summary>
        /// <para>
        /// Creates a new child lifetime scope that is as nested as possible,
        /// depending on the scope of the specified feature.
        /// </para>
        /// <para>
        /// In a SPSite or SPWeb-scoped feature context, will return a web-specific
        /// lifetime scope (allowing you to inject InstancePerSite and InstancePerWeb
        /// objects - InstancePerRequest scoped objects will be inaccessible).
        /// </para>
        /// <para>
        /// In a SPFarm or SPWebApplication feature context, will return a child
        /// container of the root application container (preventing you from injecting
        /// InstancePerSite, InstancePerWeb or InstancePerRequest objects).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <param name="feature">The current feature context from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPFeature feature)
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
        /// <para>
        /// Creates a new child lifetime scope under the scope of the specified web
        /// (allowing you to inject InstancePerSite and InstancePerWeb objects - InstancePerRequest
        /// scoped objects will be inaccessible).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <param name="web">The current web from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPWeb web)
        {
            return this.containerProvider.EnsureWebScope(web).BeginLifetimeScope();
        }

        /// <summary>
        /// <para>
        /// Creates a new child lifetime scope under the scope of the specified site collection
        /// (allowing you to inject InstancePerSite objects - InstancePerWeb and InstancePerRequest
        /// scoped objects will be inaccessible).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <param name="site">The current site collection from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPSite site)
        {
            return this.containerProvider.EnsureSiteScope(site).BeginLifetimeScope();
        }

        /// <summary>
        /// <para>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <param name="webApplication">The current context's web application</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPWebApplication webApplication)
        {
            // In this default SharePointServiceLocator implementation, the context's WebApplication gives us no 
            // useful information. We simply return a child scope of the Root application-wide container.
            return this.Current.BeginLifetimeScope();
        }

        /// <summary>
        /// <para>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// </para>
        /// <para>
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </para>
        /// </summary>
        /// <param name="farm">The current context's farm</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        public ILifetimeScope BeginLifetimeScope(SPFarm farm)
        {
            // In this default SharePointServiceLocator implementation, the context's SPFarm gives us no 
            // useful information. We simply return a child scope of the Root application-wide container.
            return this.Current.BeginLifetimeScope();
        }
    }
}
