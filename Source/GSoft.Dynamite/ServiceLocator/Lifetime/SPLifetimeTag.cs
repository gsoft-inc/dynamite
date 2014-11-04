using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core.Lifetime;

namespace GSoft.Dynamite.ServiceLocator.Lifetime
{
    /// <summary>
    /// Constants used to tag lifetime scopes within SharePoint <c>Autofac</c> web applications.
    /// </summary>
    public class SPLifetimeTag
    {
        /// <summary>
        /// Application lifetime
        /// </summary>
        public static readonly object Application = LifetimeScope.RootTag;

        /// <summary>
        /// Per-site collection lifetime
        /// </summary>
        public static readonly string Site = "spSite";

        /// <summary>
        /// Per-web lifetime
        /// </summary>
        public static readonly string Web = "spWeb";

        /// <summary>
        /// HTTP Request lifetime
        /// </summary>
        public static readonly string Request = "spRequest";
    }
}
