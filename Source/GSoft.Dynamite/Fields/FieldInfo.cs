using System;
using System.Globalization;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Defines the field info structure.
    /// </summary>
    /// <typeparam name="T">ValueType associated to that particular Field type</typeparam>
    public abstract class FieldInfo<T> : IFieldInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldInfo() : base()
        {
        }

        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="fieldTypeName">Name of the type of field (site column type)</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content Group resource key</param>
        public FieldInfo(string internalName, Guid id, string fieldTypeName, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, fieldTypeName, displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public FieldInfo(XElement fieldSchemaXml) : base(fieldSchemaXml)
        {
        }

        /// <summary>
        /// Returns the FieldInfo's associated ValueType.
        /// For example, a TextFieldInfo should return typeof(string)
        /// and a TaxonomyFieldInfo should return typeof(TaxonomyValue)
        /// </summary>
        public override Type AssociatedValueType
        {
            get
            {
                return typeof(T);
            }

            set
            {
                throw new NotSupportedException("Associated value type is determined by type argument and cannot be set.");
            }
        }

        /// <summary>
        /// Default field value.
        /// </summary>
        public T DefaultValue { get; set; }
    }
}
