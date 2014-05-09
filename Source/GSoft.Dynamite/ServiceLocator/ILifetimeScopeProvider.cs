using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Interface for the retrieval of Autofac dependency injection lifetime scopes
    /// </summary>
    public interface ILifetimeScopeProvider
    {
        /// <summary>
        /// The global root container
        /// </summary>
        IContainer ApplicationContainer { get; }

        /// <summary>
        /// Creates a new scope or returns an existing scope
        /// </summary>
        ILifetimeScope LifetimeScope { get; }

        /// <summary>
        /// Disposes a lifetime scope and all its children
        /// </summary>
        void EndLifetimeScope();
    }
}
