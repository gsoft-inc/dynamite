using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding
{
    /// <summary>
    /// A schema to apply on entities.
    /// </summary>
    public interface IEntityBindingSchema
    {
        /// <summary>
        /// The entity type described by the schema
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// The mapping details for each property of the entity
        /// </summary>
        IList<EntityPropertyConversionDetail> PropertyConversionDetails { get; }
    }
}