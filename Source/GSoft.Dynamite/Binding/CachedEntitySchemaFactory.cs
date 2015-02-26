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
    public class CachedEntitySchemaFactory : IEntitySchemaFactory
    {
        private IEntitySchemaFactory decoratedSchemaFactory;
        private ILogger logger;

        private readonly ConcurrentDictionary<Type, IEntityBindingSchema> cachedSchemas = new ConcurrentDictionary<Type, IEntityBindingSchema>();

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
    }
}
