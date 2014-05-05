using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Lifetime scope provider the help share state at the SPWeb-level
    /// </summary>
    public class SPWebLifetimeScopeProvider : ILifetimeScopeProvider
    {
        private readonly ISharePointContainerProvider containerProvider;
        private readonly SPNoDisposalLifetimeScopeHelper noDisposalLifetimeScopeHelper;

        public SPWebLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
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
        /// Creates a new scope or returns the existing scope unique to the current SPWeb.
        /// The parent scope of the new SPWeb-bound scope should be the current SPSite's
        /// own lifetime scope.
        /// </summary>
        public ILifetimeScope LifetimeScope
        {
            get 
            {
                // Throw exception if not in SPContext
                this.noDisposalLifetimeScopeHelper.ThrowExceptionIfNotSPContext();

                // Parent scope of SPSite scope is the current Site-collection-specific lifetime scope
                var parentScope = this.containerProvider.CurrentSite;
                var scopeKindTag = SPLifetime.Web;
                var childScopePerContainerUniqueKey = SPLifetime.Web + SPContext.Current.Web.ID;

                return this.noDisposalLifetimeScopeHelper.EnsureUndisposableScopeForTagInContainer(parentScope, scopeKindTag, childScopePerContainerUniqueKey);
            }
        }

        /// <summary>
        /// This implementation should be empty because SPWeb-bound scope should live
        /// as long as their parent SPSite scope, which in turn lives as long as the
        /// application container.
        /// </summary>
        public void EndLifetimeScope()
        {
            // Nothing to dispose, SPWeb scope should live as long as the root application container
        }
    }
}
