using System;
using System.Collections.Generic;
using System.Globalization;
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
                    Type associatedValueType = this.FieldInfo.AssociatedValueType;
                    bool isNullable = associatedValueType.IsGenericType && associatedValueType.Name == typeof(Nullable<>).Name;
                    Type nullableTypeArg = null;

                    if (isNullable)
                    {
                        nullableTypeArg = associatedValueType.GetGenericArguments()[0];
                    }

                    if (value.GetType() != this.FieldInfo.AssociatedValueType 
                        && value.GetType() != nullableTypeArg)
                    {
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "The type of the value (val={0} valueType={1}) must be the same as the FieldInfo AssociatedValueType (expectedFieldValueType={2}).",
                                value,
                                value.GetType(),
                                nullableTypeArg == null ? this.FieldInfo.AssociatedValueType : nullableTypeArg));
                    }
                }

                this.value = value;
            }
        }
    }
}