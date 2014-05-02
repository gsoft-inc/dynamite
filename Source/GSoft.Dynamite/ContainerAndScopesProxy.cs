// -----------------------------------------------------------------------
// <copyright file="ContainerAndScopesProxy.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Autofac;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ContainerAndScopesProxy
    {
        public readonly string appRootNamespace;
        public readonly Func<string, bool> assemblyFileNameMatcher;

        public ContainerAndScopesProxy(string appRootNamespace)
        {
            this.appRootNamespace = appRootNamespace;
        }

        public ContainerAndScopesProxy(string appRootNamespace, Func<string, bool> assemblyFileNameMatcher)
        {
            this.appRootNamespace = appRootNamespace;
            this.assemblyFileNameMatcher = assemblyFileNameMatcher;
        }

        public IContainer Root
        {
            get
            {
                return AppDomainContainers.CurrentContainer(appRootNamespace, assemblyFileNameMatcher);
            }
        }

        public ILifetimeScope Site
        {
            get
            {
                return AppDomainContainers.CurrentSiteScope(appRootNamespace, assemblyFileNameMatcher);
            }
        }

        public ILifetimeScope Web
        {
            get
            {
                return AppDomainContainers.CurrentWebScope(appRootNamespace, assemblyFileNameMatcher);
            }
        }
    }
}
