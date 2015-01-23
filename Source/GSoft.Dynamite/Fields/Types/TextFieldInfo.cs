using System;
using System.Globalization;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
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
        /// <param name="groupResourceKey">Content group resource key</param>
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
        /// Maximum number of characters in text field
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes
        /// </param>
        /// <returns>The full field XML schema</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            baseFieldSchema.Add(new XAttribute("MaxLength", this.MaxLength));

            return baseFieldSchema;
        }
    }
}
