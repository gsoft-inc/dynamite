using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using GSoft.Dynamite.ServiceLocator;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.IntegrationTests
{
    /// <summary>
    /// Service locator for Dynamite integration tests
    /// </summary>
    public static class IntegrationTestServiceLocator
    {
        private static ISharePointServiceLocator innerServiceLocator = new SharePointServiceLocator("GSoft.Dynamite");

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
        public static ILifetimeScope BeginLifetimeScope()
        {
            return innerServiceLocator.BeginLifetimeScope();
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
        public static ILifetimeScope BeginLifetimeScope(SPWeb web)
        {
            return innerServiceLocator.BeginLifetimeScope(web);
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
        public static ILifetimeScope BeginLifetimeScope(SPSite site)
        {
            return innerServiceLocator.BeginLifetimeScope(site);
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
        public static ILifetimeScope BeginLifetimeScope(SPWebApplication webApplication)
        {
            return innerServiceLocator.BeginLifetimeScope(webApplication);
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
        public static ILifetimeScope BeginLifetimeScope(SPFarm farm)
        {
            return innerServiceLocator.BeginLifetimeScope(farm);
        }
    }
}
