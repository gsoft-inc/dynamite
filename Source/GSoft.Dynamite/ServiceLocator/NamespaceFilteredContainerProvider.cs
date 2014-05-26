using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator
{
    /// <summary>
    /// Basic Autofac container provider that will automatically scan the GAC for
    /// assemblies that match the provider's appRootNamespace (or, alternatively,
    /// those that match the provider's assemblyFileMatcher).
    /// All Autoface registration modules found in those assemblies will be loaded
    /// in the provided container when Current is accessed for the first time after
    /// an application pool recyle.
    /// </summary>
    public class NamespaceFilteredContainerProvider : IContainerProvider
    {
        protected readonly string appRootNamespace;
        protected readonly Func<string, bool> assemblyFileNameMatcher;

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
        /// The assembly file name matcher (will be used instead of the appRootNamespace to
        /// match assembly names in the GAC). The appRootNamespace still acts as the provided
        /// container's unique key among all the other containers that live in the AppDomain.
        /// </param>
        public NamespaceFilteredContainerProvider(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            this.appRootNamespace = appRootNamespace;
            this.assemblyFileNameMatcher = assemblyFileNameMatcher;
        }
        
        /// <summary>
        /// A unique string to distinguish the provided container
        /// from all other containers in the current AppDomain.
        /// </summary>
        public string ContainerKey 
        {
            get
            {
                return this.appRootNamespace;
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
                return AppDomainContainers.CurrentContainer(this.appRootNamespace, this.assemblyFileNameMatcher);
            }
        }
    }
}
