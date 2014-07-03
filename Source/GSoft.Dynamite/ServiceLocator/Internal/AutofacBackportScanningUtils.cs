using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;
using GSoft.Dynamite.Extensions;

namespace GSoft.Dynamite.ServiceLocator.Internal
{
    /// <summary>
    /// Borrowed (THANKS!!) from <c>Autofac</c> repo to back port an assembly scanning fix
    /// </summary>
    public class AutofacBackportScanningUtils
    {
        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        public static void RegisterAssemblyModules(ContainerBuilder builder, params Assembly[] assemblies)
        {
            RegisterAssemblyModules<IModule>(builder, assemblies);
        }

        /// <summary>
        /// Registers modules found in an assembly.
        /// </summary>
        /// <param name="builder">Container builder.</param>
        /// <param name="assemblies">The assemblies from which to register modules.</param>
        /// <typeparam name="TModule">The type of the module to add.</typeparam>
        public static void RegisterAssemblyModules<TModule>(ContainerBuilder builder, params Assembly[] assemblies) where TModule : IModule
        {
            var moduleFinder = new ContainerBuilder();

            var moduleType = typeof(TModule);
            AutofacBackportScanningUtils.RegisterAssemblyTypes(moduleFinder, assemblies).Where(moduleType.IsAssignableFrom).As<IModule>();

            using (var moduleContainer = moduleFinder.Build())
            {
                foreach (var module in moduleContainer.Resolve<IEnumerable<IModule>>())
                {
                    builder.RegisterModule(module);
                }
            }
        }

        /// <summary>
        /// Method to register the assembly types
        /// </summary>
        /// <param name="builder">The Container used</param>
        /// <param name="assemblies">The list of assemblies to register</param>
        /// <returns>A registration builder</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            var registrationBuilder = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, registrationBuilder));

            return registrationBuilder;
        }

        private static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            rb.ActivatorData.Filters.Add(t => rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt => swt.ServiceType.IsAssignableFrom(t)));

            // The trick here is to not call a.GetTypes but rather the extension a.GetLoadableTypes
            foreach (var t in assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    !t.IsGenericTypeDefinition &&
                    rb.ActivatorData.Filters.All(p => p(t))))
            {
                var scanned = RegistrationBuilder.ForType(t)
                    .FindConstructorsWith(rb.ActivatorData.ConstructorFinder)
                    .UsingConstructor(rb.ActivatorData.ConstructorSelector)
                    .WithParameters(rb.ActivatorData.ConfiguredParameters)
                    .WithProperties(rb.ActivatorData.ConfiguredProperties);

                scanned.RegistrationData.CopyFrom(rb.RegistrationData, false);

                foreach (var action in rb.ActivatorData.ConfigurationActions)
                {
                    action(t, scanned);
                }

                if (scanned.RegistrationData.Services.Any())
                {
                    RegistrationBuilder.RegisterSingleComponent(cr, scanned);
                }
            }

            foreach (var postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
            {
                postScanningCallback(cr);
            }
        }
    }
}
