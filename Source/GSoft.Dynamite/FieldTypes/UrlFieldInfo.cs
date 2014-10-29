using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.FieldTypes
{
    /// <summary>
    /// Definition of a UrlField info
    /// </summary>
    public class UrlFieldFieldInfo : FieldInfo<UrlValue>
    {
        /// <summary>
        /// Initializes a new UrlFieldFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public UrlFieldFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "URL", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default format
            this.Format = "Hyperlink";
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public UrlFieldFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("Format") != null)
            {
                this.Format = fieldSchemaXml.Attribute("Format").Value;
            }
        }

        /// <summary>
        /// Hyperlink or Image
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("Format", this.Format));

                return schema;
            }
        }
    }
}
