namespace GSoft.Dynamite.ServiceLocator.Lifetime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Autofac;

    using Microsoft.SharePoint;

    /// <summary>
    /// Base class for all type of SharePoint Life time scopes.
    /// </summary>
    public abstract class SPLifetimeScopeProvider : ILifetimeScopeProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPLifetimeScopeProvider"/> class.
        /// </summary>
        /// <param name="containerProvider">
        /// The container provider.
        /// </param>
        protected SPLifetimeScopeProvider(ISharePointContainerProvider containerProvider)
        {
            this.ContainerProvider = containerProvider;
            this.ChildScopeFactory = new ChildScopeFactory();
        }

        /// <summary>
        /// Gets the lifetime scope abstract property.
        /// </summary>
        public abstract ILifetimeScope LifetimeScope { get; }

        /// <summary>
        /// Gets the application container.
        /// </summary>
        public IContainer ApplicationContainer
        {
            get
            {
                return this.ContainerProvider.Current;
            }
        }

        /// <summary>
        /// The child scope factory.
        /// </summary>
        internal ChildScopeFactory ChildScopeFactory { get; private set; }

        /// <summary>
        /// The SharePoint container provider.
        /// </summary>
        protected ISharePointContainerProvider ContainerProvider { get; private set; }

        /// <summary>
        /// The end lifetime scope abstract method.
        /// </summary>
        public abstract void EndLifetimeScope();

        /// <summary>
        /// The throw exception if no SPContext exists.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Throws the InvalidOperationException if context is null
        /// </exception>
        protected void ThrowExceptionIfNotSPContext()
        {
            if (SPContext.Current == null)
            {
                throw new InvalidOperationException(
                    "Can't access current a child lifetime scope for container " + this.ContainerProvider.ContainerKey + " because not in a SharePoint web request context. "
                    + "Instead, to force a sharing boundary for classes registered as InstancePerLifetimeScope, create your own lifetime scope with using(var childScope = YourRootContainer.Current.BeginLifetimeScope()) {}.");
            }
        }
    }
}
