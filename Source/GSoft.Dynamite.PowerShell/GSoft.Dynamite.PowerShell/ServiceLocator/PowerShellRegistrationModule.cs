using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Globalization;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.PowerShell.ServiceLocator
{
    /// <summary>
    /// The PowerShell registration module. We register the different type we need to use with the dependency injection engine
    /// </summary>
    public class PowerShellRegistrationModule : Module
    {
        /// <summary>
        /// Registers the module's type bindings on the container
        /// </summary>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CatalogHelper>().As<ICatalogHelper>();
        }
    }
}
