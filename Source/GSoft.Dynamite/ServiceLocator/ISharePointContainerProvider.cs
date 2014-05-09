using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Interface for the retrieval of Autofac dependency injection containers,
    /// with SharePoint-specific lifetime scopes.
    /// Prefer using a more specific/narrow lifetime scope whenever appropriate:
    /// i.e. prefer using CurrentRequest, before CurrentWeb, before Current Site, etc.
    /// Avoid using the root container Current 
    /// </summary>
    public interface ISharePointContainerProvider : IContainerProvider
    {
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
        ILifetimeScope CurrentSite { get; }

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
        ILifetimeScope CurrentWeb { get; }

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
        ILifetimeScope CurrentRequest { get; }

        /// <summary>
        /// Either creates a new lifetime scope from the specified site or
        /// returns the existing one.
        /// Don't dispose this scope instance, as it could be reused by others.
        /// Allows for the usage of InstancePerSite even when outside of 
        /// a typical http request context (for example, use EnsureSiteScope
        /// from a FeatureActivated even receiver run from Powershell.exe to
        /// reuse objects across many event receivers triggered by the same process).
        /// In typical HTTP request context, use CurrentSite property instead.
        /// </summary>
        /// <param name="site">The current site to use in retreiving or creating the scope</param>
        /// <returns>
        /// The site-collection-specific lifetime scope (a child container of 
        /// the root application one)
        /// </returns>
        ILifetimeScope EnsureSiteScope(SPSite site);

        /// <summary>
        /// Either creates a new lifetime scope from the specified web or
        /// returns the existing one.
        /// Don't dispose this scope instance, as it could be reused by others.
        /// Allows for the usage of InstancePerWeb even when outside of 
        /// a typical http request context (for example, use EnsureSiteScope
        /// from a FeatureActivated even receiver run from Powershell.exe to
        /// reuse objects across many event receivers triggered by the same process).
        /// In typical HTTP request context, use CurrentWeb property instead.
        /// </summary>
        /// <param name="web">The current web to use in retreiving or creating the scope</param>
        /// <returns>
        /// The web-specific lifetime scope (a child container of 
        /// the root application one)
        /// </returns>
        ILifetimeScope EnsureWebScope(SPWeb web);
    }
}
