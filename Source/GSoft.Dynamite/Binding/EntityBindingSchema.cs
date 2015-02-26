using System.Collections.Generic;

namespace GSoft.Dynamite.Binding
{
    using System;
using Microsoft.SharePoint;

    /// <summary>
    /// A schema to apply on entities.
    /// </summary>
    public class EntityBindingSchema : IEntityBindingSchema
    {
        public EntityBindingSchema(Type entityType)
        {
            this.PropertyConversionDetails = new List<EntityPropertyConversionDetail>();
            this.EntityType = entityType;
        }

        public Type EntityType { get; private set; }

        public IList<EntityPropertyConversionDetail> PropertyConversionDetails
        {
            get;
            private set;
        }
    }
}