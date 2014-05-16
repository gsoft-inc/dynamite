using System.Web;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Lifetime scope provider the help share state at the HTTP request level
    /// </summary>
    public class SPRequestLifetimeScopeProvider : SPLifetimeScopeProvider
    {
        /// <summary>
        /// Create a new per-HTTP-request lifetime scope so that state can be shared
        /// across a whole SPRequest.
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPRequestLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
            : base(containerProvider)
        { 
            // Subscribe our scope provider instance so that it gets notified by the HttpModule whenever the 
            // current HTTP request ends.
            RequestLifetimeHttpModule.AddRequestLifetimeScopeProvider(containerProvider.ContainerKey, this);
        }

        /// <summary>
        /// Creates a new scope or returns an existing scope
        /// </summary>
        public override ILifetimeScope LifetimeScope
        {
            get 
            {
                ILifetimeScope scope = null;
                var currentHttpRequestCacheContents = HttpContext.Current.Items[this.ScopeKeyInRequestCache];

                if (currentHttpRequestCacheContents == null)
                {
                    // Tag the child container with the "spRequest" key, so that it can be recognized
                    // for sharing across InstancePerRequest objects
                    scope = this.ContainerProvider.CurrentWeb.BeginLifetimeScope(SPLifetimeTag.Request);
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

        private string ScopeKeyInRequestCache
        {
            get
            {
                return this.ContainerProvider.ContainerKey + SPLifetimeTag.Request;
            }
        }

        /// <summary>
        /// Disposes the current HTTP request's lifetime scope and all its children.
        /// </summary>
        public override void EndLifetimeScope()
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
