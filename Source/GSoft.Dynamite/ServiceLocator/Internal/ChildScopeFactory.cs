using System;
using System.Collections.Generic;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// The ChildScopeFactory interface.
    /// </summary>
    internal interface IChildScopeFactory
    {
        /// <summary>
        /// Creates a new child scope or returns an existing child scope.
        /// </summary>
        /// <param name="parentScope">The current parent container.</param>
        /// <param name="scopeKindTag">
        /// A tag to identify this kind of scope so it can be reused to share objects 
        /// through fancy registration extensions (e.g. InstancePerSPSite, InstancePerSPWeb)
        /// </param>
        /// <param name="childScopeKey">A key to uniquely identify this scope within the container.</param>
        /// <param name="childSpecificConfigurationAction">
        /// An Autofac configuration action the will be run upon creation of the child scope 
        /// (i.e. container registrations specific to the new child scope)
        /// </param>
        /// <returns>The child scope for the uniquely identified resource</returns>
        ILifetimeScope GetChildLifetimeScope(
            ILifetimeScope parentScope,
            string scopeKindTag,
            string childScopeKey,
            Action<ContainerBuilder> childSpecificConfigurationAction);
    }

    /// <summary>
    /// Helps to maintain long-lived child containers
    /// </summary>
    internal class ChildScopeFactory : IChildScopeFactory
    {
        private readonly object childScopesLockObject = new object();

        /// <summary>
        /// Shared dictionary of container child scopes, sandboxed child containers that are meant to live as
        /// long as their parent root container
        /// </summary>
        private readonly IDictionary<string, ILifetimeScope> childScopes = new Dictionary<string, ILifetimeScope>();

        /// <summary>
        /// Creates a new child scope or returns an existing child scope.
        /// </summary>
        /// <param name="parentScope">The current parent container.</param>
        /// <param name="scopeKindTag">
        /// A tag to identify this kind of scope so it can be reused to share objects 
        /// through fancy registration extensions (e.g. InstancePerSPSite, InstancePerSPWeb)
        /// </param>
        /// <param name="childScopeKey">A key to uniquely identify this scope within the container.</param>
        /// <param name="childSpecificConfigurationAction">
        /// An Autofac configuration action the will be run upon creation of the child scope 
        /// (i.e. container registrations specific to the new child scope)
        /// </param>
        /// <returns>The child scope for the uniquely identified resource</returns>
        public ILifetimeScope GetChildLifetimeScope(
            ILifetimeScope parentScope, 
            string scopeKindTag,
            string childScopeKey, 
            Action<ContainerBuilder> childSpecificConfigurationAction)
        {
            ILifetimeScope ensuredScope = null;

            // Don't bother locking if the instance is already created
            if (this.childScopes.ContainsKey(childScopeKey))
            {
                // Return the already-initialized container right away
                ensuredScope = this.childScopes[childScopeKey];
            }
            else
            {
                // Only one scope should be registered at a time in this helper instance, to be on the safe side
                lock (this.childScopesLockObject)
                {
                    // Just in case, check again (because the assignment could have happened before we took hold of lock)
                    if (this.childScopes.ContainsKey(childScopeKey))
                    {
                        ensuredScope = this.childScopes[childScopeKey];
                    }
                    else
                    {
                        // This scope will never be disposed, i.e. it will live as long as the parent
                        // container, provided no one calls Dispose on it.
                        // The newly created scope is meant to sandbox InstancePerLifetimeScope-registered objects
                        // so that they get shared only within a boundary uniquely identified by the key.
                        ensuredScope = parentScope.BeginLifetimeScope(scopeKindTag, childSpecificConfigurationAction);
                        this.childScopes[childScopeKey] = ensuredScope;
                    }
                }
            }

            return ensuredScope;
        }
    }
}
