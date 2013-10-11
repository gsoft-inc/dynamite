using System;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// The interface for the schema builder.
    /// </summary>
    public interface IEntitySchemaBuilder
    {
        #region Methods

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The schema for the type.</returns>
        IEntitySchema GetSchema(Type type);

        /// <summary>
        /// Registers the type converter.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="converter">The converter.</param>
        void RegisterTypeConverter(Type targetType, IConverter converter);

        /// <summary>
        /// Unregisters the type converter.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        void UnregisterTypeConverter(Type targetType);

        #endregion
    }
}