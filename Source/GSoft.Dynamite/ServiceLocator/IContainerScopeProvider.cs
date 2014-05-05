// -----------------------------------------------------------------------
// <copyright file="IContainerScopeProvider.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.ServiceLocator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Autofac;

    /// <summary>
    /// The ContainerScopeProvider interface.
    /// </summary>
    public interface IContainerScopeProvider
    {
        /// <summary>
        /// Gets the root.
        /// </summary>
        IContainer Root { get; }

        /// <summary>
        /// Gets the site.
        /// </summary>
        ILifetimeScope Site { get; }

        /// <summary>
        /// Gets the web.
        /// </summary>
        ILifetimeScope Web { get; }
    }
}
