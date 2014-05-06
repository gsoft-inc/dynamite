using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Lifetime scope provider the help share state at the SPSite-level
    /// </summary>
    public class SPSiteLifetimeScopeProvider : SPLifetimeScopeProvider
    {
        /// <summary>
        /// Creates a new per-SPSite lifetime scope provider so that state can be shared
        /// throughout the app's lifetime on a per-site-collection basis.
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPSiteLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
            : base(containerProvider)
        { 
        }

        /// <summary>
        /// Creates a new scope or returns the existing scope unique to the current SPSite.
        /// The parent scope of the new SPSite-bound scope should be the root application container.
        /// </summary>
        public override ILifetimeScope LifetimeScope
        {
            get 
            {
                // Throw exception if not in SPContext
                this.ThrowExceptionIfNotSPContext();

                return this.EnsureSiteScopeInternal(SPContext.Current.Site);
            }
        }

        /// <summary>
        /// Ensure the creation of a site-collection-specific lifetime scope (or reuse an existing one).
        /// Don't dispose this instance, as it is meant to live as long as the root app container.
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <returns>The current site-collection-specific lifetime scope</returns>
        public ILifetimeScope EnsureSiteScope(SPSite site)
        {
            return this.EnsureSiteScopeInternal(site);
        }

        /// <summary>
        /// Disposes a lifetime scope and all its children.
        /// This implementation should be empty because SPSite-bound scope should live
        /// as long as the application container.
        /// </summary>
        public override void EndLifetimeScope()
        {
            // Nothing to dispose, SPSite scope should live as long as the root application container
        }

        private ILifetimeScope EnsureSiteScopeInternal(SPSite site)
        {
            // Parent scope of SPSite scope is the Root application container
            var parentScope = this.ContainerProvider.Current;
            var scopeKindTag = SPLifetimeTag.Site;
            var childScopePerSiteContainerUniqueKey = scopeKindTag + site.ID;

            return this.ChildScopeFactory.GetChildLifetimeScope(parentScope, scopeKindTag, childScopePerSiteContainerUniqueKey);
        }
    }
}
