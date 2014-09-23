using System;
using System.Data;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using Microsoft.Office.Server.ApplicationRegistry.MetadataModel;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Defines the field info structure.
    /// </summary>
    public abstract class FieldInfo<T> : BaseTypeInfo, IFieldInfo
    {
        #region Properties backing fields

        private string _internalName;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public FieldInfo()
        {
        }

        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        public FieldInfo(string internalName, Guid id, string sharePointFieldTypeName)
        {
            if (string.IsNullOrEmpty(internalName))
            {
                throw new ArgumentNullException("internalName");
            } 
            else if (id == null || id == Guid.Empty) 
            {
                throw new ArgumentNullException("internalName");
            }
            else if (internalName.Length > 32)
            {
                throw new ArgumentOutOfRangeException("internalName", "SharePoint field internal name cannot have more than 32 characters");
            }

            this.InternalName = internalName;
            this.Id = id;
            this.Type = sharePointFieldTypeName;
        }

        /// <summary>
        /// Unique identifier of the field
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// The internal name of the field
        /// </summary>
        public string InternalName { get; private set; }

        /// <summary>
        /// SharePoint Field Type name of the field
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Indicates if the field is required
        /// </summary>
        public RequiredTypes Required { get; set; }

        /// <summary>
        /// Indicates if the field must enforce unique values
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// Returns the FieldInfo's associated ValueType.
        /// For example, a TextFieldInfo should return typeof(string)
        /// and a TaxonomyFieldInfo should return typeof(TaxonomyValue)
        /// </summary>
        public Type AssociatedValueType
        {
            get
            {
                return typeof(T);
            }
        }


        /// <summary>
        /// Default field value.
        /// </summary>
        public T DefaultValue { get; set; }

        /// <summary>
        /// The XML schema of the field
        /// </summary>
        public abstract XElement Schema { get; }

        /// <summary>
        /// The string XML format of the field
        /// </summary>
        /// <returns>The XML schema of the field as string</returns>
        public override string ToString()
        {
            return this.Schema.ToString();
        }
    }
}
