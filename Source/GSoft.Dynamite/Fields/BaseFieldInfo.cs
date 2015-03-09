using System;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using Newtonsoft.Json;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Basic metadata about a SharePoint field/site column
    /// </summary>
    public class BaseFieldInfo : BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public BaseFieldInfo() : base()
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
        public BaseFieldInfo(string internalName, Guid id, string fieldTypeName, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            if (string.IsNullOrEmpty(internalName))
            {
                throw new ArgumentNullException("internalName");
            }
            else if (id == null || id == Guid.Empty)
            {
                throw new ArgumentNullException("id");
            }
            else if (internalName.Length > 32)
            {
                throw new ArgumentOutOfRangeException("internalName", "SharePoint field internal name cannot have more than 32 characters");
            }

            this.InternalName = internalName;
            this.Id = id;
            this.FieldType = fieldTypeName;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public BaseFieldInfo(XElement fieldSchemaXml)
        {
            if (fieldSchemaXml == null)
            {
                throw new ArgumentNullException("fieldSchemaXml");
            }

            if (!XmlHasAllBasicAttributes(fieldSchemaXml))
            {
                throw new ArgumentException("Attribute missing from field definitions: ID, Name or Type.", "fieldSchemaXml");
            }

            this.Id = new Guid(fieldSchemaXml.Attribute("ID").Value);
            this.InternalName = fieldSchemaXml.Attribute("Name").Value;
            this.FieldType = fieldSchemaXml.Attribute("Type").Value;

            if (fieldSchemaXml.Attribute("DisplayName") != null)
            {
                // TODO: maybe try to parse $Resource string here... maybe not?
                this.DisplayNameResourceKey = fieldSchemaXml.Attribute("DisplayName").Value;
            }

            if (fieldSchemaXml.Attribute("Description") != null)
            {
                // TODO: maybe try to parse $Resource string here... maybe not?
                this.DescriptionResourceKey = fieldSchemaXml.Attribute("Description").Value;
            }

            if (fieldSchemaXml.Attribute("Group") != null)
            {
                // TODO: maybe try to parse $Resource string here... maybe not?
                this.GroupResourceKey = fieldSchemaXml.Attribute("Group").Value;
            }

            if (fieldSchemaXml.Attribute("Required") != null)
            {
                this.Required = bool.Parse(fieldSchemaXml.Attribute("Required").Value) ? RequiredType.Required : RequiredType.NotRequired;
            }

            if (fieldSchemaXml.Attribute("EnforceUniqueValues") != null)
            {
                this.EnforceUniqueValues = bool.Parse(fieldSchemaXml.Attribute("EnforceUniqueValues").Value);
            }

            if (fieldSchemaXml.Attribute("Hidden") != null)
            {
                this.IsHidden = bool.Parse(fieldSchemaXml.Attribute("Hidden").Value);
            }

            if (fieldSchemaXml.Attribute("ShowInDisplayForm") != null)
            {
                this.IsHiddenInDisplayForm = !bool.Parse(fieldSchemaXml.Attribute("ShowInDisplayForm").Value);
            }

            if (fieldSchemaXml.Attribute("ShowInEditForm") != null)
            {
                this.IsHiddenInEditForm = !bool.Parse(fieldSchemaXml.Attribute("ShowInEditForm").Value);
            }

            if (fieldSchemaXml.Attribute("ShowInNewForm") != null)
            {
                this.IsHiddenInNewForm = !bool.Parse(fieldSchemaXml.Attribute("ShowInNewForm").Value);
            }

            if (fieldSchemaXml.Attribute("ShowInListSettings") != null)
            {
                this.IsHiddenInListSettings = !bool.Parse(fieldSchemaXml.Attribute("ShowInListSettings").Value);
            }

            if (fieldSchemaXml.Attribute("DefaultFormula") != null)
            {
                this.DefaultFormula = fieldSchemaXml.Attribute("DefaultFormula").Value;
            }
        }

        /// <summary>
        /// Unique identifier of the field
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The internal name of the field
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Type of the field
        /// </summary>
        public string FieldType { get; set; }
        
        /// <summary>
        /// Indicates if the field is required
        /// </summary>
        public RequiredType Required { get; set; }

        /// <summary>
        /// Indicates if the field must enforce unique values
        /// </summary>
        public bool EnforceUniqueValues { get; set; }

        /// <summary>
        /// Returns the FieldInfo's associated ValueType.
        /// For example, a TextFieldInfo should return typeof(string)
        /// and a TaxonomyFieldInfo should return typeof(TaxonomyValue)
        /// </summary>
        [JsonIgnore]
        public virtual Type AssociatedValueType
        {
            get;
            private set;
        }

        /// <summary>
        /// Full name of the field's associated value type, convenient
        /// for serialization.
        /// </summary>
        public string AssociatedValueTypeAsString
        {
            get
            {
                return this.AssociatedValueType.FullName;
            }

            set
            {
                this.AssociatedValueType = Type.GetType(value);
            }
        }

        /// <summary>
        /// Indicates if field is hidden by default
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the display form
        /// </summary>
        public bool IsHiddenInDisplayForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the new form
        /// </summary>
        public bool IsHiddenInNewForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the edit form
        /// </summary>
        public bool IsHiddenInEditForm { get; set; }

        /// <summary>
        /// Indicates if field should be shown in the list settings
        /// </summary>
        public bool IsHiddenInListSettings { get; set; }

        /// <summary>
        /// Default formula for the field
        /// </summary>
        public string DefaultFormula { get; set; }

        /// <summary>
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes
        /// </param>
        /// <returns>The full field XML schema</returns>
        public virtual XElement Schema(XElement baseFieldSchema)
        {
            throw new NotSupportedException("Can't use Schema method on BaseFieldInfo object. Use a field type that derives from FieldInfoWithValueType<T> instead.");
        }

        private static bool XmlHasAllBasicAttributes(XElement fieldSchemaXml)
        {
            return fieldSchemaXml.Attribute("ID") != null
                || fieldSchemaXml.Attribute("Name") != null
                || fieldSchemaXml.Attribute("Type") != null;
        }
    }
}
