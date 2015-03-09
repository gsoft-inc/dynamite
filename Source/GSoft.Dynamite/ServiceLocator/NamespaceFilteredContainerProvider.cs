using System;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Basic <c>Autofac</c> container provider that will automatically scan the GAC for
    /// assemblies that match the provider's AppRootNamespace (or, alternatively,
    /// those that match the provider's AssemblyFileMatcher).
    /// All <c>Autofac</c> registration modules found in those assemblies will be loaded
    /// in the provided container when Current is accessed for the first time after
    /// an application pool recycle.
    /// </summary>
    public class NamespaceFilteredContainerProvider : IContainerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerScopeProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        public NamespaceFilteredContainerProvider(string appRootNamespace) : this(appRootNamespace, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerScopeProvider"/> class.
        /// </summary>
        /// <param name="appRootNamespace">
        /// The app root namespace.
        /// </param>
        /// <param name="assemblyFileNameMatcher">
        /// The assembly file name matcher (will be used instead of the AppRootNamespace to
        /// match assembly names in the GAC). The AppRootNamespace still acts as the provided
        /// container's unique key among all the other containers that live in the AppDomain.
        /// </param>
        public NamespaceFilteredContainerProvider(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            this.AppRootNamespace = appRootNamespace;
            this.AssemblyFileNameMatcher = assemblyFileNameMatcher;
        }

        /// <summary>
        /// A unique string to distinguish the provided container
        /// from all other containers in the current AppDomain.
        /// </summary>
        public string ContainerKey 
        {
            get
            {
                return this.AppRootNamespace;
            }
        }

        /// <summary>
        /// Returns the current global application-wide container.
        /// Whenever applicable, prefer creating a child lifetime scope instead of resolving 
        /// directly for this root Container instance.
        /// </summary>
        public IContainer Current
        {
            get
            {
                return AppDomainContainers.CurrentContainer(this.AppRootNamespace, this.AssemblyFileNameMatcher);
            }
        }

        /// <summary>
        /// The App Root namespace
        /// </summary>
        protected string AppRootNamespace { get; private set; }

        /// <summary>
        /// The Assembly file matcher
        /// </summary>
        protected Func<string, bool> AssemblyFileNameMatcher { get; private set; }
    }
}
