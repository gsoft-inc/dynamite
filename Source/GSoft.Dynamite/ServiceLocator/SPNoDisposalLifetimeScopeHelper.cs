using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Helps to maintain long-lived child containers that depend on the current SPContext
    /// </summary>
    internal class SPNoDisposalLifetimeScopeHelper
    {
        private readonly ISharePointContainerProvider containerProvider;

        private readonly object ChildScopesLockObject = new object();

        /// <summary>
        /// Shared dictionary of container child scopes, sandboxed child containers that are meant to live as
        /// long as their parent root container
        /// </summary>
        private readonly IDictionary<string, ILifetimeScope> uniqueChildScopes = new Dictionary<string, ILifetimeScope>();

        /// <summary>
        /// Creates a new helper
        /// </summary>
        /// <param name="containerProvider">The current container provider</param>
        public SPNoDisposalLifetimeScopeHelper(ISharePointContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
        }

        /// <summary>
        /// Creates a new child scope or returns an existing child scope.
        /// </summary>
        /// <param name="parentScope">The current parent container.</param>
        /// <param name="scopeKindTag">
        /// A tag to identify this kind of scope so it can be reused to share objects 
        /// through fancy registration extensions (e.g. InstancePerSPSite, InstancePerSPWeb)
        /// </param>
        /// <param name="childScopePerContainerUniqueKey">A key to uniquely identify this</param>
        /// <returns>The child scope for the uniquely identified resource</returns>
        public ILifetimeScope EnsureUndisposableScopeForTagInContainer(ILifetimeScope parentScope, string scopeKindTag, string childScopePerContainerUniqueKey)
        {
            ILifetimeScope ensuredScope = null;

            var fullScopeKindTag = this.containerProvider.ContainerUniqueKey + "-" + scopeKindTag;
            var childScopeUniqueKey = this.containerProvider.ContainerUniqueKey + "-" + childScopePerContainerUniqueKey;

            // Don't bother locking if the instance is already created
            if (uniqueChildScopes.ContainsKey(childScopeUniqueKey))
            {
                // Return the already-initialized container right away
                ensuredScope = uniqueChildScopes[childScopeUniqueKey];
            }
            else
            {
                // Only one scope should be registered at a time in this helper instance, to be on the safe side
                lock (this.ChildScopesLockObject)
                {
                    // Just in case, check again (because the assignment could have happened before we took hold of lock)
                    if (uniqueChildScopes.ContainsKey(childScopeUniqueKey))
                    {
                        ensuredScope = uniqueChildScopes[childScopeUniqueKey];
                    }
                    else
                    {
                        // This scope will never be disposed, i.e. it will life as long as the parent
                        // container, provided no one calls Dispose on it.
                        // The newly created scope is meant to sandbox InstancePerLifetimeScope-registered objects
                        // so that they get shared only within a boundary uniquely identified by the key.
                        ensuredScope = parentScope.BeginLifetimeScope(fullScopeKindTag);
                        uniqueChildScopes[childScopeUniqueKey] = ensuredScope;
                    }
                }
            }

            return ensuredScope;
        }

        public void ThrowExceptionIfNotSPContext()
        {
            if (SPContext.Current == null)
            {
                throw new InvalidOperationException(
                    "Can't access current a child lifetime scope for container " + this.containerProvider.ContainerUniqueKey + " because not in a SharePoint web request context. "
                    + "Instead, to force a sharing boundary for classes registered as InstancePerLifetimeScope, create your own lifetime scope with using(var childScope = YourRootContainer.Current.BeginLifetimeScope()) {}.");
            }
        }
    }
}
