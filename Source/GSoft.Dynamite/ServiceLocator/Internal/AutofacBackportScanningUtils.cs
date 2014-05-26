namespace GSoft.Dynamite.ServiceLocator.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Autofac;
    using System.Reflection;
    using Autofac.Core;
    using Autofac.Builder;
    using Autofac.Features.Scanning;
    using GSoft.Dynamite.Extensions;

    /// <summary>
    /// Borrowed (THANKS!!) from Autofac repo to backport an assembly scanning fix
    /// </summary>
    public class AutofacBackportScanningUtils
    {
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
        public static void RegisterAssemblyModules<TModule>(ContainerBuilder builder, params Assembly[] assemblies)
            where TModule : IModule
        {
            var moduleFinder = new ContainerBuilder();

            var moduleType = typeof(TModule);
            AutofacBackportScanningUtils.RegisterAssemblyTypes(moduleFinder, assemblies)
                .Where(moduleType.IsAssignableFrom)
                .As<IModule>();

            using (var moduleContainer = moduleFinder.Build())
            {
                foreach (var module in moduleContainer.Resolve<IEnumerable<IModule>>())
                    builder.RegisterModule(module);
            }
        }

        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterAssemblyTypes(ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            var rb = new RegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>(
                new TypedService(typeof(object)),
                new ScanningActivatorData(),
                new DynamicRegistrationStyle());

            builder.RegisterCallback(cr => ScanAssemblies(assemblies, cr, rb));

            return rb;
        }

        static void ScanAssemblies(IEnumerable<Assembly> assemblies, IComponentRegistry cr, IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> rb)
        {
            rb.ActivatorData.Filters.Add(t =>
                rb.RegistrationData.Services.OfType<IServiceWithType>().All(swt =>
                    swt.ServiceType.IsAssignableFrom(t)));

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
                    action(t, scanned);

                if (scanned.RegistrationData.Services.Any())
                    RegistrationBuilder.RegisterSingleComponent(cr, scanned);
            }

            foreach (var postScanningCallback in rb.ActivatorData.PostScanningCallbacks)
                postScanningCallback(cr);
        }
    }
}
