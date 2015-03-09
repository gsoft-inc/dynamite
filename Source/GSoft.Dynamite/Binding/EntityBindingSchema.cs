using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// A schema to apply on entities.
    /// </summary>
    public class EntityBindingSchema : IEntityBindingSchema
    {
        /// <summary>
        /// Creates a new <see cref="EntityBindingSchema"/>
        /// </summary>
        /// <param name="entityType">The entity type described by the schema</param>
        public EntityBindingSchema(Type entityType)
        {
            this.PropertyConversionDetails = new List<EntityPropertyConversionDetail>();
            this.EntityType = entityType;
        }

        /// <summary>
        /// The entity type described by the schema
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// The mapping details for each property of the entity
        /// </summary>
        public IList<EntityPropertyConversionDetail> PropertyConversionDetails
        {
            get;
            private set;
        }
    }
}