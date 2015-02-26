using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.ValueTypes.Readers
{
    /// <summary>
    /// Marker inteface for implementations of BaseValueReader of T
    /// </summary>
    public interface IBaseValueReader
    {
        /// <summary>
        /// The value type this reader knows how to read from SharePoint elements
        /// </summary>
        Type AssociatedValueType { get; }
    }
}
