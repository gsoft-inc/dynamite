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
    public class SPSiteLifetimeScopeProvider : ILifetimeScopeProvider
    {
        private readonly ISharePointContainerProvider containerProvider;
        private readonly SPNoDisposalLifetimeScopeHelper noDisposalLifetimeScopeHelper;

        /// <summary>
        /// Creates a new per-SPSite lifetime scope provider so that state can be shared
        /// throughout the app's lifetime on a per-site-collection basis.
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPSiteLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            this.noDisposalLifetimeScopeHelper = new SPNoDisposalLifetimeScopeHelper(this.containerProvider);
        }

        /// <summary>
        /// The global root container
        /// </summary>
        public IContainer ApplicationContainer
        {
            get 
            { 
                return this.containerProvider.Current; 
            }
        }

        /// <summary>
        /// Creates a new scope or returns the existing scope unique to the current SPSite.
        /// The parent scope of the new SPSite-bound scope should be the root application container.
        /// </summary>
        public ILifetimeScope LifetimeScope
        {
            get 
            {
                // Throw exception if not in SPContext
                this.noDisposalLifetimeScopeHelper.ThrowExceptionIfNotSPContext();

                // Parent scope of SPSite scope is the Root application container
                var parentScope = this.containerProvider.Current;
                var scopeKindTag = SPLifetime.Site;
                var childScopePerContainerUniqueKey = SPLifetime.Site + SPContext.Current.Site.ID;

                return this.noDisposalLifetimeScopeHelper.EnsureUndisposableScopeForTagInContainer(parentScope, scopeKindTag, childScopePerContainerUniqueKey);
            }
        }

        /// <summary>
        /// Disposes a lifetime scope and all its children.
        /// This implementation should be empty because SPSite-bound scope should live
        /// as long as the application container.
        /// </summary>
        public void EndLifetimeScope()
        {
            // Nothing to dispose, SPSite scope should live as long as the root application container
        }
    }
}
