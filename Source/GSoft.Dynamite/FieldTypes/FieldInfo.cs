using System;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using Microsoft.Office.Server.ApplicationRegistry.MetadataModel;
using System.Globalization;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Defines the field info structure.
    /// </summary>
    public abstract class FieldInfo<T> : BaseTypeInfo, IFieldInfo
    {
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
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public FieldInfo(string internalName, Guid id, string fieldTypeName, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
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
            this.Type = fieldTypeName;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public FieldInfo(XElement fieldSchemaXml)
        {
            if (fieldSchemaXml == null)
            {
                throw new ArgumentNullException("fieldSchemaXml");
            }
            
            if (!this.XmlHasAllBasicAttributes(fieldSchemaXml))
            {
                throw new ArgumentException("fieldSchemaXml", "Attribute missing from field definitions: ID, Name or Type.");
            }

            this.Id = new Guid(fieldSchemaXml.Attribute("ID").Value);
            this.InternalName = fieldSchemaXml.Attribute("Name").Value;
            this.Type = fieldSchemaXml.Attribute("Type").Value;

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
                this.Required = bool.Parse(fieldSchemaXml.Attribute("Required").Value) ? RequiredTypes.Required : RequiredTypes.NotRequired;
            }

            if (fieldSchemaXml.Attribute("EnforceUniqueValues") != null)
            {
                this.EnforceUniqueValues = bool.Parse(fieldSchemaXml.Attribute("EnforceUniqueValues").Value);
            }

            if (fieldSchemaXml.Attribute("Hidden") != null)
            {
                this.IsHidden = bool.Parse(fieldSchemaXml.Attribute("Hidden").Value);
            }
        }

        private bool XmlHasAllBasicAttributes(XElement fieldSchemaXml)
        {
            return fieldSchemaXml.Attribute("ID") != null
                || fieldSchemaXml.Attribute("Name") != null
                || fieldSchemaXml.Attribute("Type") != null;
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
        /// Indicates if field should be hidden
        /// </summary>
        public bool IsHidden { get; set; }

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

        protected XElement BasicFieldSchema
        {
            get
            {
                var schema = new XElement(
                    "Field",
                    new XAttribute("Name", this.InternalName),
                    new XAttribute("Type", this.Type),
                    new XAttribute("ID", "{" + this.Id + "}"),
                    new XAttribute("StaticName", this.InternalName),
                    new XAttribute("DisplayName", this.DisplayName),
                    new XAttribute("Description", this.Description),
                    new XAttribute("Group", this.Group),
                    new XAttribute("EnforceUniqueValues", this.EnforceUniqueValues.ToString().ToUpper()),
                    new XAttribute("ShowInListSettings", "TRUE"));
                
                // Check the Required type
                if (this.Required == RequiredTypes.Required)
                {
                    schema.Add(new XAttribute("Required", "TRUE"));
                }

                if (this.Required == RequiredTypes.NotRequired)
                {
                    schema.Add(new XAttribute("Required", "FALSE"));
                }

                // Hidden state
                if (this.IsHidden)
                {
                    schema.Add(new XAttribute("Hidden", "TRUE"));
                }

                return schema;
            }
        }
    }
}
