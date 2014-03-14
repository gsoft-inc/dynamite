using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Core;
using GSoft.Dynamite.DI.Autofac;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.DependencyInjectors
{
    /// <summary>
    /// The general RegistrationModuleContainer interface.
    /// </summary>
    public interface IRegistrationModuleContainer
    {
        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <remarks>
        /// This is a convenience method meant to save us the hassle of always depending on the
        /// usual IUnityContain.Resolve extension method from Microsoft.Practices.Unity, which
        /// forces us to always refer to that namespace.
        /// </remarks>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <returns>The implementation of the type specified</returns>
        T Resolve<T>();

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <param name="name">The name of the registration</param>
        /// <returns>The implementation of the type specified</returns>
        T Resolve<T>(string name);
    }

    /// <summary>
    /// Modularized Autofac container
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class AutofacRegistrationModuleContainer : IRegistrationModuleContainer
    {
        private const string AutofacType = "Autofac.Module";
        private const string AssemblyFolder = "GAC_MSIL";

        private readonly IContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacRegistrationModuleContainer"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public AutofacRegistrationModuleContainer(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Creates a new Autofac container with the Dynamite registration module
        /// pre-configured. Also scans the GAC to retrieve any DLL matching the 
        /// specified predicate and auto-register any Autofac registration module
        /// found within.
        /// </summary>
        /// <param name="assemblyNameMatchingPredicate"></param>
        /// <param name="logCategoryName">Logging category name with which the Dynamite <see cref="TraceLogger"/> will log to the Unified Logging System</param>
        /// <param name="defaultResourceFileNames">Namespaces for the various resource files needed by the parent Application so that Dynamite's <see cref="IResourceLocator"/> knows where to hunt for resources</param>
        /// <returns></returns>
        public static AutofacRegistrationModuleContainer ScanGacForAutofacModulesAndCreateContainer(Func<string, bool> assemblyNameMatchingPredicate, string logCategoryName, string[] defaultResourceFileNames)
        {
            var containerBuilder = new ContainerBuilder();

            var assemblyLocator = new GacAssemblyLocator();

            var matchingAssemblies = assemblyLocator.GetAssemblies(new List<string>() { AssemblyFolder }, assemblyNameMatchingPredicate);

            var abstractType = RetrieveAutofacAbstractModuleType(AppDomain.CurrentDomain.GetAssemblies(), AutofacType);

            if (abstractType == null)
            {
                throw new ArgumentNullException(string.Format("Abstract Type {0} not found in current assemblies.", AutofacType));
            }

            foreach (var assembly in matchingAssemblies)
            {
                // Don't register anything from the current DLL (we'll take care registering the Dynamite Registration Module ourselves below)
                if (!assembly.FullName.Contains("GSoft.Dynamite.DI.Autofac"))
                {
                    var types = assembly.GetTypes()
                        .Where(
                            myType =>
                            myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(abstractType));

                    foreach (Type type in types)
                    {
                        var module = assembly.CreateInstance(type.FullName);
                        containerBuilder.RegisterModule((IModule)module);
                    }
                }
            }
           
            // Don't just scan the GAC modules, also prepare the Dynamite core utils (by passing the params in ourselves)
            var dynamiteModule = new AutofacDynamiteRegistrationModule(logCategoryName, defaultResourceFileNames);
            containerBuilder.RegisterModule(dynamiteModule);

            var containerInstance = new AutofacRegistrationModuleContainer(containerBuilder.Build());
            containerInstance.AssemblyMatchingPredicate = assemblyNameMatchingPredicate;

            return containerInstance;
        }

        /// <summary>
        /// Scans the GAC for the first type that implements the specified interface and registers the pair the Container
        /// </summary>
        /// <param name="containerToUpdate">The Autofac dependency injection container to update with the new registration</param>
        /// <param name="interfaceType">Interface type that needs an implementation to register</param>
        public static void ScanAssembliesForInterfaceImplementationAndRegisterFirstMatch(Func<string, bool> assemblyNameMatchingPredicate, AutofacRegistrationModuleContainer containerToUpdate, Type interfaceType)
        {
            ScanAssembliesForInterfaceImplementationAndRegisterFirstMatch(assemblyNameMatchingPredicate, containerToUpdate, new List<Type>() { interfaceType });
        }

        /// <summary>
        /// Scans the GAC for the first types that implements the specified interfaces and registers the pairs the Container
        /// </summary>
        /// <param name="containerToUpdate">The Autofac dependency injection container to update with the new registrations</param>
        /// <param name="interfaceTypes">Interface types that each need an implementation to register</param>
        public static void ScanAssembliesForInterfaceImplementationAndRegisterFirstMatch(Func<string, bool> assemblyNameMatchingPredicate, AutofacRegistrationModuleContainer containerToUpdate, IList<Type> interfaceTypes)
        {
            if (interfaceTypes == null)
            {
                throw new ArgumentNullException("interfaceTypes");
            }

            // Accumulate (Interface, Implementation) pairs so that we can register them all at once at the end
            var typePairsToRegister = new Dictionary<Type, Type>();

            foreach (var interfaceType in interfaceTypes)
            {
                bool foundImplementation = false;

                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException(string.Format("Specified Type {0} should be an interface.", interfaceType));
                }

                var assemblyLocator = new GacAssemblyLocator();
                var logger = containerToUpdate.Resolve<ILogger>();

                var matchingAssemblies = assemblyLocator.GetAssemblies(new List<string>() { AssemblyFolder }, assemblyNameMatchingPredicate);

                foreach (var assembly in matchingAssemblies)
                {
                    var types = assembly.GetTypes().Where(myType => myType.IsClass && interfaceType.IsAssignableFrom(myType)).ToList();

                    if (types.Count > 1)
                    {
                        logger.Warn("More than one type found that implements the interface {0}. First one ({1}) will be used.", interfaceType, types.First());
                    }

                    if (types.Count > 0)
                    {
                        typePairsToRegister.Add(interfaceType, types.First());
                        foundImplementation = true;

                        // Don't look any further for this specific interface
                        break;
                    }
                }

                if (!foundImplementation)
                {
                    logger.Warn("Failed to find any type that implements {0} in AppDomain GAC_MSIL assemblies.", interfaceType);
                }
            }

            if (typePairsToRegister.Count > 0)
            {
                var autofacInnerContainer = containerToUpdate.InnerAutofacContainerInstance;
                var containerBuilder = new ContainerBuilder();

                foreach (var interfaceType in typePairsToRegister.Keys)
                {
                    // The first type we find that implements the interface is chosen automatically
                    var implementationType = typePairsToRegister[interfaceType];
                    containerBuilder.RegisterType(implementationType).As(interfaceType);
                }

                containerBuilder.Update(autofacInnerContainer);
            }
        }

        /// <summary>
        /// Exposes the inner Autofac container instance
        /// </summary>
        public IContainer InnerAutofacContainerInstance
        {
            get
            {
                return this.container;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<string, bool> AssemblyMatchingPredicate
        {
            get;
            set;
        }

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <remarks>
        /// This is a convenience method meant to save us the hassle of always depending on the
        /// usual IUnityContain.Resolve extension method from Microsoft.Practices.Unity, which
        /// forces us to always refer to that namespace.
        /// </remarks>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <returns>The implementation of the type specified</returns>
        public T Resolve<T>()
        {
            return this.container.Resolve<T>();
        }

        /// <summary>
        /// Resolves the registered implementation for the specified type
        /// </summary>
        /// <typeparam name="T">The type for which we want an implementation</typeparam>
        /// <param name="name">The name of the registration</param>
        /// <returns>The implementation of the type specified</returns>
        public T Resolve<T>(string name)
        {
            return this.container.ResolveNamed<T>(name);
        }

        private static Type RetrieveAutofacAbstractModuleType(IEnumerable<Assembly> assemblies, string autofacType)
        {
            Type abstractType = null;
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(autofacType);

                if (type == null)
                {
                    continue;
                }
                else
                {
                    abstractType = type;
                    break;
                }
            }

            return abstractType;
        }
    }
}
