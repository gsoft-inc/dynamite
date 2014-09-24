using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;
using GSoft.Dynamite.Definitions;

namespace GSoft.Dynamite.FieldTypes
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
