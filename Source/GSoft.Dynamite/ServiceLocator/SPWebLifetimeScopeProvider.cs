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
    public class SPWebLifetimeScopeProvider : SPLifetimeScopeProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPWebLifetimeScopeProvider"/> class.
        /// </summary>
        /// <param name="containerProvider">
        /// The container provider.
        /// </param>
        public SPWebLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
            : base(containerProvider)
        {
        }

        /// <summary>
        /// Creates a new scope or returns the existing scope unique to the current SPWeb.
        /// The parent scope of the new SPWeb-bound scope should be the current SPSite's
        /// own lifetime scope.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If called from a non-http-request context. Use EnsureWebScope to force the creation
        /// of web lifetime scopes outside of a web request context.
        /// </exception>
        public override ILifetimeScope LifetimeScope
        {
            get 
            {
                // Throw exception if not in SPContext
                this.ThrowExceptionIfNotSPContext();

                return this.EnsureWebScopeInternal(SPContext.Current.Web);
            }
        }

        /// <summary>
        /// Ensure the creation of a web-specific lifetime scope (or reuse an existing one).
        /// Don't dispose this instance, as it is meant to live as long as the root app container.
        /// </summary>
        /// <param name="web">The current web</param>
        /// <returns>The current web-specific lifetime scope</returns>
        public ILifetimeScope EnsureWebScope(SPWeb web)
        {
            return this.EnsureWebScopeInternal(web);
        }

        /// <summary>
        /// This implementation should be empty because SPWeb-bound scope should live
        /// as long as their parent SPSite scope, which in turn lives as long as the
        /// application container.
        /// </summary>
        public override void EndLifetimeScope()
        {
            // Nothing to dispose, SPWeb scope should live as long as the root application container
        }

        private ILifetimeScope EnsureWebScopeInternal(SPWeb web)
        {
            // Parent scope of SPWeb scope is the current Site-collection-specific lifetime scope
            var parentScope = this.ContainerProvider.CurrentSite;
            var scopeKindTag = SPLifetimeTag.Web;
            var childContainerKey = scopeKindTag + web.ID;

            return this.ChildScopeFactory.GetChildLifetimeScope(parentScope, scopeKindTag, childContainerKey);
        }
    }
}
