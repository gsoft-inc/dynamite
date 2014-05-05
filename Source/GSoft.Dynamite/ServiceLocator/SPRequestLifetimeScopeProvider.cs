using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;
using System.Web;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Lifetime scope provider the help share state at the HTTP request level
    /// </summary>
    public class SPRequestLifetimeScopeProvider : ILifetimeScopeProvider
    {
        private readonly ISharePointContainerProvider containerProvider;

        /// <summary>
        /// Create a new per-HTTP-request lifetime scope so that state can be shared
        /// across a whole SPRequest.
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPRequestLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;

            // Subscribe our scope provider instance so that it gets notified by the HttpModule whenever the 
            // current HTTP request ends.
            SPRequestLifetimeHttpModule.AddRequestLifetimeScopeProvider(containerProvider.ContainerUniqueKey, this);
        }

        private string ScopeKeyInRequestCache
        {
            get
            {
                return this.containerProvider.ContainerUniqueKey + SPLifetime.Request;
            }
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
        /// Creates a new scope or returns an existing scope
        /// </summary>
        public ILifetimeScope LifetimeScope
        {
            get 
            {
                ILifetimeScope scope = null;
                var currentHttpRequestCacheContents = HttpContext.Current.Items[this.ScopeKeyInRequestCache];

                if (currentHttpRequestCacheContents == null)
                {
                    // Tag the child container with the "spRequest" key, so that it can be recognized
                    // for sharing across InstancePerRequest objects
                    scope = this.containerProvider.CurrentWeb.BeginLifetimeScope(SPLifetime.Request);
                    HttpContext.Current.Items[this.ScopeKeyInRequestCache] = scope;
                }
                else
                {
                    // Extract the existing scope from the current HTTP request cache
                    scope = (ILifetimeScope)currentHttpRequestCacheContents;
                }

                return scope;
            }
        }

        /// <summary>
        /// Disposes the current HTTP request's lifetime scope and all its children.
        /// </summary>
        public void EndLifetimeScope()
        {
            var currentHttpRequestCacheContents = HttpContext.Current.Items[this.ScopeKeyInRequestCache];

            if (currentHttpRequestCacheContents != null)
            {
                // Only bother trying to end the lifetime if one actually exists
                var scope = (ILifetimeScope)currentHttpRequestCacheContents;
                scope.Dispose();
            }
        }
    }
}
