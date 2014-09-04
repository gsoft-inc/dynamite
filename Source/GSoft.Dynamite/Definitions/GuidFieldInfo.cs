using System.Xml.Linq;

namespace GSoft.Dynamite.Definitions
{
    public class GuidFieldInfo: FieldInfo
    {
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
