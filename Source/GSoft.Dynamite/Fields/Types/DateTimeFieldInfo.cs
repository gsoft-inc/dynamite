using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a DateTime info
    /// </summary>
    public class DateTimeFieldInfo : BaseFieldInfoWithValueType<DateTime?>
    {
        /// <summary>
        /// Initializes a new DateTimeFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public DateTimeFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "DateTime", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            // default format
            this.Format = DateTimeFieldFormat.DateOnly;
            this.HasFriendlyRelativeDisplay = false;
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public DateTimeFieldInfo(XElement fieldSchemaXml)
            : base(fieldSchemaXml)
        {
            if (fieldSchemaXml.Attribute("Format") != null)
            {
                this.Format = fieldSchemaXml.Attribute("Format").Value == DateTimeFieldFormat.DateTime.ToString() ?
                    DateTimeFieldFormat.DateTime : DateTimeFieldFormat.DateOnly;
            }

            if (fieldSchemaXml.Attribute("FriendlyDisplayFormat") != null
                && fieldSchemaXml.Attribute("FriendlyDisplayFormat").Value == "Relative")
            {
                this.HasFriendlyRelativeDisplay = true;
            }
        }

        /// <summary>
        /// Format can be DateTime or DateOnly
        /// </summary>
        public DateTimeFieldFormat Format { get; set; }

        /// <summary>
        /// Toggle to show a friendly relative-time string instead of timestamp
        /// </summary>
        public bool HasFriendlyRelativeDisplay { get; set; }

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
            baseFieldSchema.Add(new XAttribute("Format", this.Format.ToString()));

            if (this.HasFriendlyRelativeDisplay)
            {
                baseFieldSchema.Add(new XAttribute("FriendlyDisplayFormat", "Relative"));
            }
            else
            {
                baseFieldSchema.Add(new XAttribute("FriendlyDisplayFormat", "Disabled"));
            }

            return baseFieldSchema;
        }
    }
}
