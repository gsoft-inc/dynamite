using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Binding
{
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
