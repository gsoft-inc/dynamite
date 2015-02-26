using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// Builds the schema of entity mapping details for all of an entity's properties
    /// tagged with the PropertyAttribute.
    /// </summary>
    public interface IEntitySchemaFactory
    {
        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The schema for the type.</returns>
        IEntityBindingSchema GetSchema(Type type);
    }
}
