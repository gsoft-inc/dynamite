using System;
using GSoft.Dynamite.Sharepoint.Collections;

namespace GSoft.Dynamite.Sharepoint.Binding
{
    /// <summary>
    /// A cached schema builder.
    /// </summary>
    public class CachedSchemaBuilder : IEntitySchemaBuilder, IDisposable
    {
        #region Fields

        private readonly IEntitySchemaBuilder _schemaBuilder;

        private readonly ConcurrentDictionary<Type, IEntitySchema> _cachedSchemas = new ConcurrentDictionary<Type, IEntitySchema>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedSchemaBuilder"/> class.
        /// </summary>
        /// <param name="schemaBuilder">The schema builder.</param>
        public CachedSchemaBuilder(IEntitySchemaBuilder schemaBuilder)
        {
            this._schemaBuilder = schemaBuilder;
        }

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
            IEntitySchema schema;
            if (!this._cachedSchemas.TryGetValue(type, out schema))
            {
                schema = this._schemaBuilder.GetSchema(type);
                this._cachedSchemas.Add(type, schema);
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
            this._schemaBuilder.RegisterTypeConverter(targetType, converter);
        }

        /// <summary>
        /// Unregisters the type converter.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        public void UnregisterTypeConverter(Type targetType)
        {
            this._schemaBuilder.UnregisterTypeConverter(targetType);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool managed)
        {
            this._cachedSchemas.Dispose();
        }

        #endregion
    }
}