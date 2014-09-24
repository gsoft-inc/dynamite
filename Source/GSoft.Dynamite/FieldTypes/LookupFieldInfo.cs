using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a LookupFieldInfo
    /// </summary>
    public class LookupFieldInfo : FieldInfo<LookupValue>
    {
        /// <summary>
        /// Initializes a new LookupFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public LookupFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Lookup", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default lookup displayed field
            this.ShowField = "Title";
            this.ListId = Guid.Empty;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public LookupFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("ShowField") != null)
            {
                this.ShowField = fieldSchemaXml.Attribute("ShowField").Value;
            }

            if (fieldSchemaXml.Attribute("List") != null)
            {
                this.ListId = Guid.Parse(fieldSchemaXml.Attribute("List").Value);
            }
        }

        /// <summary>
        /// The internal name of the field of which we want to see the value in the lookup
        /// </summary>
        public string ShowField { get; set; }

        /// <summary>
        /// The looked-up list identifier
        /// </summary>
        public Guid ListId { get; set; }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("List", "{" + this.ListId + "}"));
                schema.Add(new XAttribute("ShowField", this.ShowField));

                return schema;
            }
        }
    }
}
