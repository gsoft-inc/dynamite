using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GSoft.Dynamite.Collections;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes.Readers;
using GSoft.Dynamite.ValueTypes.Writers;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// Decorator for <see cref="EntitySchemaFactory"/> which takes care of caching
    /// entity mapping details for all types.
    /// </summary>
    public class CachedEntitySchemaFactory : IEntitySchemaFactory, IDisposable
    {
        private readonly ConcurrentDictionary<Type, IEntityBindingSchema> cachedSchemas = new ConcurrentDictionary<Type, IEntityBindingSchema>();

        private IEntitySchemaFactory decoratedSchemaFactory;
        private ILogger logger;

        /// <summary>
        /// Creates a new instance of <see cref="CachedEntitySchemaFactory"/>
        /// </summary>
        /// <param name="decoratedSchemaFactory">The real implementation of the schema factory</param>
        /// <param name="logger">Logging utility</param>
        public CachedEntitySchemaFactory(IEntitySchemaFactory decoratedSchemaFactory, ILogger logger)
        {
            this.decoratedSchemaFactory = decoratedSchemaFactory;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The schema for the type.</returns>
        public IEntityBindingSchema GetSchema(Type type)
        {
            IEntityBindingSchema schema;
            if (!this.cachedSchemas.TryGetValue(type, out schema))
            {
                try
                {
                    schema = this.decoratedSchemaFactory.GetSchema(type);
                    this.cachedSchemas.Add(type, schema);
                }
                catch (ArgumentException exception)
                {
                    this.logger.Warn("Entity Binding Concurrency Conflict - Schema was already added for type " + type.FullName + " - Exception: " + exception.ToString());

                    // The schema was cached by a concurrent thread already, use the already stored value instead of the useless duplicate 
                    // we just created with the schema builder
                    schema = this.cachedSchemas[type];
                }
            }

            return schema;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool managed)
        {
            this.cachedSchemas.Dispose();
        }

        #endregion
    }
}
