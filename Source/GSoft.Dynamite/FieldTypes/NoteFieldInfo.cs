using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using System.Globalization;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a NoteField info
    /// </summary>
    public class NoteFieldInfo : FieldInfo<string>
    {
        /// <summary>
        /// Initializes a new NoteFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public NoteFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "Note", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default number of lines shown when editing
            this.NumLines = 6;
            this.HasRichText = false;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public NoteFieldInfo(XElement fieldSchemaXml) : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("NumLines") != null)
            {
                this.NumLines = int.Parse(fieldSchemaXml.Attribute("NumLines").Value, CultureInfo.InvariantCulture);
            }

            if (fieldSchemaXml.Attribute("RichText") != null
                && bool.Parse(fieldSchemaXml.Attribute("RichText").Value))
            {
                this.HasRichText = true;
            }
        }

        /// <summary>
        /// Number of lines shown when editing
        /// </summary>
        public int NumLines { get; set; }

        /// <summary>
        /// RichText toggle
        /// </summary>
        public bool HasRichText { get; set; }

        /// <summary>
        /// The XML schema of the Note field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                var schema = this.BasicFieldSchema;

                schema.Add(new XAttribute("NumLines", this.NumLines));

                if (this.HasRichText)
                {
                    schema.Add(new XAttribute("RichText", "TRUE"));
                    schema.Add(new XAttribute("RichTextMode", "FullHtml"));
                }
                else
                {
                    schema.Add(new XAttribute("RichText", "FALSE"));
                    schema.Add(new XAttribute("RichTextMode", "Compatible"));
                }

                return schema;
            }
        }
    }
}
