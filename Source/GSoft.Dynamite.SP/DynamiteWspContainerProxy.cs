using Autofac;
using GSoft.Dynamite.ServiceLocator;
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite
{
    internal class DynamiteWspContainerProxy
    {
        private ISharePointServiceLocator innerLocator = new AddOnProvidedServiceLocator();
        
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
                return innerLocator.Current;
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
        /// Prefer usage of this method versus resolving manually from the Current property.
        /// </summary>
        /// <param name="feature">The current feature that is requesting a child lifetime scope</param>
        /// <returns>A new child lifetime scope which should be disposed by the caller.</returns>
        ILifetimeScope BeginFeatureLifetimeScope(SPFeature feature)
        {
            return innerLocator.BeginFeatureLifetimeScope(feature);
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
        ILifetimeScope BeginWebLifetimeScope(SPWeb web)
        {
            return innerLocator.BeginWebLifetimeScope(web);
        }
    }
}
