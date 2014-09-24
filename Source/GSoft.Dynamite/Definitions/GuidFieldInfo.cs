using System.Xml.Linq;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a GUID field
    /// </summary>
    public class GuidFieldInfo : FieldInfo
    {
        /// <summary>
        /// The XML schema of a GUID field as XElement (override)
        /// </summary>
        /// <returns>The XML schema of a GUID field as XElement</returns>
        public override XElement ToXElement()
        {
            this.Schema = new XElement(
                                    "Field",
                                    new XAttribute("Name", this.InternalName),
                                    new XAttribute("Type", "Guid"),
                                    new XAttribute("ID", "{" + this.Id + "}"),
                                    new XAttribute("StaticName", this.StaticName),
                                    new XAttribute("DisplayName", this.DisplayName),
                                    new XAttribute("Description", this.Description),
                                    new XAttribute("Group", this.Group));

            return this.Schema;
        }
    }
}
