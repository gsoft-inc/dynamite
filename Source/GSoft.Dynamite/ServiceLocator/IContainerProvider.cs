using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{   
    /// <summary>
    /// Interface for the retrieval of Autofac dependency injection containers
    /// </summary>
    public interface IContainerProvider
    {
        /// <summary>
        /// A unique string to distinguish the provided container
        /// from all other containers in the current AppDomain.
        /// </summary>
        string ContainerUniqueKey { get; }

        /// <summary>
        /// Returns the current global application-wide container.
        /// Whenever applicable, prefer creating a child lifetime scope instead of resolving 
        /// directly for this root Container instance.
        /// </summary>
        IContainer Current { get; }
    }
}
