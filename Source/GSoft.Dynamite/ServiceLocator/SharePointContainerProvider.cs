using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using GSoft.Dynamite.ServiceLocator.Lifetime;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{  
    /// <summary>
    /// A SharePoint-specific <c>Autofac</c> container provider implementation, which is meant 
    /// to provide your application with a Container that automatically scans the GAC
    /// (using the AppRootNamespace or AssemblyFileNameMatcher as assembly filename
    /// filter) and loads the matched assemblies' registration modules.
    /// </summary>
    /// <remarks>
    /// In your application, create and maintain your own <see cref="SharePointContainerProvider"/>
    /// instance to act as Service Locator for your entire application.
    /// Avoid using the same AppRootNamespace with two different provider instances,
    /// as the same AppDomain-wide inner container instance will be reused in that case
    /// (details in <see cref="AppDomainContainers"/> ).
    /// </remarks>
    /// <example>
    /// How to share all instances registered with InstancePerLifetimeScope throughout
    /// the current request (requires the <see cref="RequestLifetimeHttpModule"/> to
    /// be deployed to your web.config):
    /// <![CDATA[ 
    /// var myPerRequestCache = provider.CurrentRequest.Resolve<ISomePerRequestCache>();
    /// ]]>
    /// Similarly, object sharing scoped to a site collection:
    /// <![CDATA[ 
    /// var userService = provider.CurrentSite.Resolve<IUserService>();
    /// ]]>
    /// Using Dynamite utilities from a feature event receiver:
    /// <![CDATA[ 
    /// var currentSite = properties.Feature.Parent as SPSite;
    /// using (var siteScope = provider.EnsureSiteScope(currentSite))
    /// {
    ///     var logger = siteScope.Resolve<ILogger>();
    ///     var taxonomyService = siteScope.Resolve<ITaxonomyService>();
    ///     taxonomyService.GetTermForId(currentSite, Guid.NewsGuid());
    ///     logger.Info("Tough luck!");
    /// }
    /// ]]>
    /// </example>
    public class SharePointContainerProvider : NamespaceFilteredContainerProvider, ISharePointContainerProvider
    {
        private readonly SPSiteLifetimeScopeProvider siteLifetimeScopeProvider;
        private readonly SPWebLifetimeScopeProvider webLifetimeScopeProvider;
        private readonly ILifetimeScopeProvider requestLifetimeScopeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointContainerProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        public SharePointContainerProvider(string appRootNamespace) : this(appRootNamespace, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharePointContainerProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher (will be used instead of the AppRootNamespace to
        /// match assembly names in the GAC). The AppRootNamespace still acts as the provided
        /// container's unique key among all the other containers that live in the AppDomain.
        /// </param>
        public SharePointContainerProvider(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher) : base(appRootNamespace, assemblyFileNameMatcher)
        {
            this.siteLifetimeScopeProvider = new SPSiteLifetimeScopeProvider(this);
            this.webLifetimeScopeProvider = new SPWebLifetimeScopeProvider(this);
            this.requestLifetimeScopeProvider = new SPRequestLifetimeScopeProvider(this);
        }
        
        /// <summary>
        /// A lifetime scope reserved for the current context's SPSite.
        /// Use to inject and share InstancePerLifetimeScope-registered objects 
        /// across all interactions with the current site collection.
        /// Classes registered with the InstancePerSPSite registration extension
        /// will be shared through this scope.
        /// Should be a direct child scope of the global application container.
        /// This scope should not be disposed manually: it is meant to live as long
        /// as its parent.
        /// Do not use outside typical HTTP request context (use EnsureSiteScope instead).
        /// </summary>
        public ILifetimeScope CurrentSite
        {
            get 
            { 
                return this.siteLifetimeScopeProvider.LifetimeScope; 
            }
        }

        /// <summary>
        /// A lifetime scope reserved for the current context's SPWeb.
        /// Use to inject and share InstancePerLifetimeScope-registered objects 
        /// across all interactions with the current SharePoint web.
        /// Classes registered with the InstancePerSPWeb registration extension
        /// will be shared through this scope.
        /// Should be a direct child scope of the CurrentSite lifetime scope.
        /// This scope should not be disposed manually: it is meant to live as long
        /// as its parent.
        /// Do not use outside typical HTTP request context (use EnsureWebScope instead).
        /// </summary>
        public ILifetimeScope CurrentWeb
        {
            get
            {
                return this.webLifetimeScopeProvider.LifetimeScope;
            }
        }

        /// <summary>
        /// A lifetime scope reserved for the current context's HTTP request.
        /// Use to inject and share InstancePerLifetimeScope-registered objects 
        /// across all interactions within the current SPRequest.
        /// Classes registered with the InstancePerSPRequest registration extension
        /// will be shared through this scope.
        /// Should be a direct child scope of the CurrentWeb lifetime scope.
        /// This scope should not be disposed manually: the <see cref="SharePointRequestAutofacHttpModule"/>
        /// should be the one to take care of its automatic disposal.
        /// </summary>
        /// <remarks>
        /// Depends on the successful deployment and configuration of the <see cref="SharePointRequestAutofacHttpModule"/>
        /// </remarks>
        public ILifetimeScope CurrentRequest
        {
            get 
            { 
                return this.requestLifetimeScopeProvider.LifetimeScope;
            }
        }

        /// <summary>
        /// Either creates a new lifetime scope from the specified site or
        /// returns the existing one.
        /// Don't dispose this scope instance, as it could be reused by others.
        /// Allows for the usage of InstancePerSite even when outside of 
        /// a typical HTTP request context (for example, use EnsureSiteScope
        /// from a FeatureActivated even receiver run from PowerShell.exe to
        /// reuse objects across many event receivers triggered by the same process).
        /// In typical HTTP request context, use CurrentSite property instead.
        /// </summary>
        /// <param name="site">The current site to use in retrieving or creating the scope</param>
        /// <returns>
        /// The site-collection-specific lifetime scope (a child container of 
        /// the root application one)
        /// </returns>
        public ILifetimeScope EnsureSiteScope(SPSite site)
        {
            return this.siteLifetimeScopeProvider.EnsureSiteScope(site);
        }

        /// <summary>
        /// Either creates a new lifetime scope from the specified web or
        /// returns the existing one.
        /// Don't dispose this scope instance, as it could be reused by others.
        /// Allows for the usage of InstancePerWeb even when outside of 
        /// a typical http request context (for example, use EnsureSiteScope
        /// from a FeatureActivated even receiver run from PowerShell.exe to
        /// reuse objects across many event receivers triggered by the same process).
        /// In typical HTTP request context, use CurrentWeb property instead.
        /// </summary>
        /// <param name="web">The current web to use in retrieving or creating the scope</param>
        /// <returns>
        /// The web-specific lifetime scope (a child container of 
        /// the root application one)
        /// </returns>
        public ILifetimeScope EnsureWebScope(SPWeb web)
        {
            return this.webLifetimeScopeProvider.EnsureWebScope(web);
        }
    }
}
