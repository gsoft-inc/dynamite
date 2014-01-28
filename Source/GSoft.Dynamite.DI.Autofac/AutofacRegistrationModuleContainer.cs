// -----------------------------------------------------------------------
// <copyright file="AutofacRegistrationModule.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.DependencyInjectors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    using Autofac;
    using Autofac.Core;

    /// <summary>
    /// The UnityRegistrationModuleContainer interface.
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
            return this.container.Resolve<T>(name);
        }
    }
}
