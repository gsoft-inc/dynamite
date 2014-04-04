using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Represents a GUID field schema
    /// </summary>
    public class GuidFieldSchema : GenericFieldSchema
    {
        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public override XElement ToXElement()
        {
            this.FieldSchema = new XElement(
                "Field",
                new XAttribute("Name", this.FieldName),
                new XAttribute("Type", "Guid"),
                new XAttribute("ID", "{" + this.FieldId + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup));

            return this.FieldSchema;
        }

        /// <summary>
        /// Get the XML schema as string of the field.
        /// </summary>
        /// <returns>A string that represents the XML schema.</returns>
        public override string ToString()
        {
            return this.ToXElement().ToString();
        }
    }
}
