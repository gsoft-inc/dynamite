// -----------------------------------------------------------------------
// <copyright file="LifetimeScopeProvider.cs" company="">
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

    using Microsoft.SharePoint;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SPLifetimeScopeProvider : IDisposable
    {
        private readonly IContainer rootContainer;

        private Dictionary<object, ILifetimeScope> lifetimeScopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPLifetimeScopeProvider"/> class.
        /// </summary>
        /// <param name="rootContainer">
        /// The root Container.
        /// </param>
        public SPLifetimeScopeProvider(IContainer rootContainer)
        {
            this.rootContainer = rootContainer;
            this.lifetimeScopes = new Dictionary<object, ILifetimeScope>();
        }

        public IContainer Current
        {
            get
            {
                return this.rootContainer;
            }
        }

        /// <summary>
        /// Gets the web application.
        /// </summary>
        public ILifetimeScope WebApplication
        {
            get
            {
                return this.Current.BeginLifetimeScope(SPContext.Current.Site.WebApplication);
            }
        }

        /// <summary>
        /// Gets the site.
        /// </summary>
        public ILifetimeScope Site
        {
            get
            {
                return this.Current.BeginLifetimeScope(SPContext.Current.Site.ID);
            }
        }

        /// <summary>
        /// Gets the web.
        /// </summary>
        public ILifetimeScope Web
        {
            get
            {
                return this.Current.BeginLifetimeScope(SPContext.Current.Web.ID);
            }
        }

        /// <summary>
        /// The externally controlled scope.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="ILifetimeScope"/>.
        /// </returns>
        public ILifetimeScope ExternallyControlledScope(object key)
        {
            return this.Current.BeginLifetimeScope(key);
        }

        public void Dispose()
        {
            // Dispose all lifetimescopes
            foreach (var lifetimeScope in this.lifetimeScopes.Values)
            {
                lifetimeScope.Dispose();
            }
        }
    }
}
