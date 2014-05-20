// -----------------------------------------------------------------------
// <copyright file="AutofacBackportAutowiringPropertyInjector.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.ServiceLocator.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Autofac;
    using System.Reflection;

    /// <summary>
    /// Borrowed (THANKS!!) from Autofac repo to backport an assembly scanning fix
    /// </summary>
    internal class AutowiringPropertyInjector
    {
        public void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (instance == null) throw new ArgumentNullException("instance");

            var instanceType = instance.GetType();

            foreach (var property in instanceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.CanWrite))
            {
                var propertyType = property.PropertyType;

                if (propertyType.IsValueType && !propertyType.IsEnum)
                    continue;

                if (property.GetIndexParameters().Length != 0)
                    continue;

                if (!context.IsRegistered(propertyType))
                    continue;

                var accessors = property.GetAccessors(false);
                if (accessors.Length == 1 && accessors[0].ReturnType != typeof(void))
                    continue;

                if (!overrideSetValues &&
                    accessors.Length == 2 &&
                    (property.GetValue(instance, null) != null))
                    continue;

                var propertyValue = context.Resolve(propertyType);
                property.SetValue(instance, propertyValue, null);
            }
        }
    }
}
