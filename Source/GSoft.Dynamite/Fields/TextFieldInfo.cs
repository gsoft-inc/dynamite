using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;

namespace GSoft.Dynamite.Fields
{
    /// <summary>
    /// Definition of a TextField info
    /// </summary>
    public class TextFieldInfo : FieldInfo<string>
    {
        /// <summary>
        /// Initializes a new TextFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public TextFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Text", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // Default max length
            this.MaxLength = 255;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public TextFieldInfo(XElement fieldSchemaXml) : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("MaxLength") != null)
            {
                this.MaxLength = int.Parse(fieldSchemaXml.Attribute("MaxLength").Value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Maxinimum number of characters in text field
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// The XML schema of the Text field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("MaxLength", this.MaxLength));

                return schema;
            }
        }
    }
}
