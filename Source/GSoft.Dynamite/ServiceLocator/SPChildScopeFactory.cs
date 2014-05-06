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
        /// The get Child life time scope.
        /// </summary>
        /// <param name="parentScope">
        /// The parent scope.
        /// </param>
        /// <param name="childScopeKey">
        /// The child scope per container unique key.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        ILifetimeScope GetChildLifeTimeScope(
            ILifetimeScope parentScope,
            string childScopeKey);
    }

    /// <summary>
    /// Helps to maintain long-lived child containers that depend on the current SPContext
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
        /// <param name="childScopeKey">A key to uniquely identify this scope</param>
        /// <returns>The child scope for the uniquely identified resource</returns>
        public ILifetimeScope GetChildLifeTimeScope(ILifetimeScope parentScope, string childScopeKey)
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
                        ensuredScope = parentScope.BeginLifetimeScope(childScopeKey);
                        this.childScopes[childScopeKey] = ensuredScope;
                    }
                }
            }

            return ensuredScope;
        }
    }
}
