namespace GSoft.Dynamite.ServiceLocator
{
    using System;

    using Autofac;

    /// <summary>
    /// The container scope provider.
    /// </summary>
    public class ContainerScopeProvider : IContainerScopeProvider
    {
        private readonly string appRootNamespace;
        private readonly Func<string, bool> assemblyFileNameMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerScopeProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        public ContainerScopeProvider(string appRootNamespace)
        {
            this.appRootNamespace = appRootNamespace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerScopeProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher.
        /// </param>
        public ContainerScopeProvider(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            this.appRootNamespace = appRootNamespace;
            this.assemblyFileNameMatcher = assemblyFileNameMatcher;
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        public IContainer Root
        {
            get
            {
                return AppDomainContainers.CurrentContainer(this.appRootNamespace, this.assemblyFileNameMatcher);
            }
        }

        /// <summary>
        /// Gets the site scope.
        /// </summary>
        public ILifetimeScope Site
        {
            get
            {
                return AppDomainContainers.CurrentSiteScope(this.appRootNamespace, this.assemblyFileNameMatcher);
            }
        }

        /// <summary>
        /// Gets the web scope.
        /// </summary>
        public ILifetimeScope Web
        {
            get
            {
                return AppDomainContainers.CurrentWebScope(this.appRootNamespace, this.assemblyFileNameMatcher);
            }
        }
    }
}
