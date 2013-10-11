using System;
using System.Linq;
using System.Reflection;
using GSoft.Dynamite.Collections;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// The default builder for the schema.
    /// </summary>
    /// <typeparam name="T">The type for the entity schema.</typeparam>
    public class EntitySchemaBuilder<T> : IEntitySchemaBuilder, IDisposable
        where T : EntitySchema, new()
    {
        #region Fields

        private readonly ConcurrentDictionary<Type, IConverter> _registeredTypes = new ConcurrentDictionary<Type, IConverter>();

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEntitySchemaBuilder Members

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The schema for the type.
        /// </returns>
        public IEntitySchema GetSchema(Type type)
        {
            var schema = new T();

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var customAttributes = property.GetCustomAttributes(typeof(PropertyAttribute), false);
                var propertyDetails = customAttributes.OfType<PropertyAttribute>().FirstOrDefault();
                if (propertyDetails != null)
                {
                    var valueKey = !string.IsNullOrEmpty(propertyDetails.PropertyName) ? propertyDetails.PropertyName : property.Name;
                    var converter = propertyDetails.CreateConverter() ?? this.GetConverterForType(property.PropertyType);

                    schema.AddProperty(new EntityBindingDetail(property, valueKey, converter, propertyDetails.BindingType));
                }
            }

            return schema;
        }

        /// <summary>
        /// Registers the type converter.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="converter">The converter.</param>
        public void RegisterTypeConverter(Type targetType, IConverter converter)
        {
            this._registeredTypes.Add(targetType, converter);
        }

        /// <summary>
        /// Unregisters the type converter.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public void UnregisterTypeConverter(Type targetType)
        {
            this._registeredTypes.Remove(targetType);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the converter for.
        /// </summary>
        /// <param name="type">The type to get the converter for.</param>
        /// <returns>The converter for the type.</returns>
        protected internal virtual IConverter GetConverterForType(Type type)
        {
            IConverter converter;

            if (!this._registeredTypes.TryGetValue(type, out converter))
            {
                converter = StraightConverter.Instance;
            }

            return converter;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool managed)
        {
            this._registeredTypes.Dispose();
        }

        #endregion
    }
}