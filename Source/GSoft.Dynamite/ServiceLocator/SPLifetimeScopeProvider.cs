namespace GSoft.Dynamite.ServiceLocator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Autofac;

    using Microsoft.SharePoint;

    /// <summary>
    /// Base classe for all type of SharePoint Life time scopes.
    /// </summary>
    public abstract class SPLifetimeScopeProvider : ILifetimeScopeProvider
    {
        /// <summary>
        /// The SharePoint container provider.
        /// </summary>
        protected readonly ISharePointContainerProvider containerProvider;

        /// <summary>
        /// The child scope factory.
        /// </summary>
        internal readonly ChildScopeFactory childScopeFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPLifetimeScopeProvider"/> class.
        /// </summary>
        /// <param name="containerProvider">
        /// The container provider.
        /// </param>
        /// <param name="childScopeFactory">
        /// The child scope factory.
        /// </param>
        protected SPLifetimeScopeProvider(
            ISharePointContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            this.childScopeFactory = new ChildScopeFactory();
        }

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
                    "Can't access current a child lifetime scope for container " + this.containerProvider.ContainerKey + " because not in a SharePoint web request context. "
                    + "Instead, to force a sharing boundary for classes registered as InstancePerLifetimeScope, create your own lifetime scope with using(var childScope = YourRootContainer.Current.BeginLifetimeScope()) {}.");
            }
        }

        /// <summary>
        /// Gets the application container.
        /// </summary>
        public IContainer ApplicationContainer
        {
            get
            {
                return this.containerProvider.Current; 
            }
        }

        public abstract ILifetimeScope LifetimeScope { get; }

        public abstract void EndLifetimeScope();
    }
}
