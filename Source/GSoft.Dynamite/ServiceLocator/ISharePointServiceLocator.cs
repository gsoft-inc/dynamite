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
    // Interface for the retrieval of Autofac dependency injection lifetime scopes,
    // with SharePoint-specific semantics.
    // Less flexible than <see cref="ISharePointContainerProvider">, it is meant to
    // encourage container usage that depends as little as possible on direct injection 
    // through the service locator pattern.
    /// </summary>
    public interface ISharePointServiceLocator
    {
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
        /// InstancePerWeb or InstancePerRequest objects).
        /// 
        /// Do not dispose this scope, as it will be reused by others. Prefer using
        /// BeginLifetimeScope() within a using block to this method to ensure all
        /// IDisposable objects you inject get properly disposed.
        /// </summary>
        [Obsolete("Prefer usage of BeginLifetimeScope() from a using block to ensure proper disposal of all IDisposable objects you injected.")]
        ILifetimeScope Current { get; }        

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
        /// or InstancePerRequest objects).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope();

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
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="feature">The current feature context from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope(SPFeature feature);

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified web
        /// (allowing you to inject InstancePerSite and InstancePerWeb objects - InstancePerRequest
        /// scoped objects will be inaccessible).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="web">The current web from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope(SPWeb web);

        /// <summary>
        /// Creates a new child lifetime scope under the scope of the specified site collection
        /// (allowing you to inject InstancePerSite objects - InstancePerWeb and InstancePerRequest
        /// scoped objects will be inaccessible).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="site">The current site collection from which we are requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope(SPSite site);

        /// <summary>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="webApplication">The current context's web application</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope(SPWebApplication webApplication);

        /// <summary>
        /// Creates a new child lifetime scope under the root application container (objects
        /// registered as InstancePerSite, InstancePerWeb or InstancePerRequest will be
        /// inaccessible).
        /// 
        /// Please dispose this lifetime scope when done (E.G. call this method from
        /// a using block).
        /// </summary>
        /// <param name="site">The current context's farm</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginLifetimeScope(SPFarm farm);
    }
}
