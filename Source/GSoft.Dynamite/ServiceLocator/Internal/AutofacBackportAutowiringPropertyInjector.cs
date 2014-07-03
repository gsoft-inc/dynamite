using System;
using System.Linq;
using System.Reflection;
using Autofac;

namespace GSoft.Dynamite.ServiceLocator.Internal
{
    /// <summary>
    /// Borrowed (THANKS!!) from <c>Autofac</c> repo to back port an assembly scanning fix
    /// </summary>
    internal class AutowiringPropertyInjector
    {
        /// <summary>
        /// The method to inject properties into a context
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="instance">The instance of the object</param>
        /// <param name="overrideSetValues">Override values</param>
        public void InjectProperties(IComponentContext context, object instance, bool overrideSetValues)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            var instanceType = instance.GetType();

            foreach (var property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(pi => pi.CanWrite))
            {
                var propertyType = property.PropertyType;

                if (propertyType.IsValueType && !propertyType.IsEnum)
                {
                    continue;
                }

                if (property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                if (!context.IsRegistered(propertyType))
                {
                    continue;
                }

                var accessors = property.GetAccessors(false);
                if (accessors.Length == 1 && accessors[0].ReturnType != typeof(void))
                {
                    continue;
                }

                if (!overrideSetValues && accessors.Length == 2 && (property.GetValue(instance, null) != null))
                {
                    continue;
                }

                var propertyValue = context.Resolve(propertyType);
                property.SetValue(instance, propertyValue, null);
            }
        }
    }
}
