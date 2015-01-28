using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Fields.Types
{
    /// <summary>
    /// Definition of a HtmlFieldInfo info. Represents a Publishing HTML field definition.
    /// </summary>
    public class HtmlFieldInfo : FieldInfo<string>
    {
        /// <summary>
        /// Initializes a new HtmlFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public HtmlFieldInfo(string internalName, Guid id, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(internalName, id, "HTML", displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
        }

        /// <summary>
        /// Creates a new FieldInfo object from an existing field schema XML
        /// </summary>
        /// <param name="fieldSchemaXml">Field's XML definition</param>
        public HtmlFieldInfo(XElement fieldSchemaXml) : base(fieldSchemaXml)
        {
        }

        /// <summary>
        /// Extends a basic XML schema with the field type's extra attributes
        /// </summary>
        /// <param name="baseFieldSchema">
        /// The basic field schema XML (Id, InternalName, DisplayName, etc.) on top of which 
        /// we want to add field type-specific attributes (RichText=TRUE and RichTextMode=ThemeHtml,
        /// as should be the default for HTML type fields)
        /// </param>
        /// <returns>The full field XML schema</returns>
        public override XElement Schema(XElement baseFieldSchema)
        {
            baseFieldSchema.Add(new XAttribute("RichText", "TRUE"));
            baseFieldSchema.Add(new XAttribute("RichTextMode", "ThemeHtml"));
                
            return baseFieldSchema;
        }
    }
}
