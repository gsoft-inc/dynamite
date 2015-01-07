using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Associate a value with field information. This can then be used for things like setting values on list items.
    /// </summary>
    public class FieldValueInfo
    {
        private object value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueInfo"/> class.
        /// </summary>
        /// <param name="fieldInfo">The field information for which the value will be set to.</param>
        /// <param name="value">The value to set to the field. The type of the object must be the same as the FieldInfo AssociatedValueType.</param>
        public FieldValueInfo(IFieldInfo fieldInfo, object value)
        {
            this.FieldInfo = fieldInfo;
            this.Value = value;
        }

        /// <summary>
        /// The field information for which the value will be set to.
        /// </summary>
        public IFieldInfo FieldInfo { get; set; }

        /// <summary>
        /// The value to set to the field. The type of the object must be the same as the FieldInfo AssociatedValueType.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">When the type of the value is not the same as the FieldInfo AssociatedValueType.</exception>
        public object Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (value != null)
                {
                    if (value.GetType() != this.FieldInfo.AssociatedValueType)
                    {
                        throw new InvalidOperationException("The type of the value must be the same as the FieldInfo AssociatedValueType.");
                    }
                }

                this.value = value;
            }
        }
    }
}