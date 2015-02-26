using System.Collections.Generic;

namespace GSoft.Dynamite.Binding
{
    using System;
    using Microsoft.SharePoint;

    /// <summary>
    /// A schema to apply on entities.
    /// </summary>
    public interface IEntityBindingSchema
    {
        Type EntityType { get; }

        IList<EntityPropertyConversionDetail> PropertyConversionDetails { get; }
    }
}