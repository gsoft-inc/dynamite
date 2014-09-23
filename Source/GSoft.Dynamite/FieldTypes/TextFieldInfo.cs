using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;

namespace GSoft.Dynamite.Definitions
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
        public TextFieldInfo(string internalName, Guid id) : base(internalName, id, "Text")
        {
        }

        /// <summary>
        /// The XML schema of the Text field
        /// </summary>
        public override XElement Schema
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
                    new XAttribute("ShowInListSettings", "TRUE"));

                // Check the Required type
                if (this.Required == RequiredTypes.Required)
                {
                    this.Schema.Add(new XAttribute("Required", "TRUE"));
                }

                if (this.Required == RequiredTypes.NotRequired)
                {
                    this.Schema.Add(new XAttribute("Required", "FALSE"));
                }

                return schema;
            }
        }
    }
}
