using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Definitions.Values
{
    /// <summary>
    /// Definition a generic FieldInfoValue
    /// </summary>
    /// <typeparam name="T">The value type corresponding to the field</typeparam>
    public class FieldInfoValue<T> : IFieldInfoValue
    {
        /// <summary>
        /// Values for a field
        /// </summary>
        public T[] Values { get; set; }
    }
}
