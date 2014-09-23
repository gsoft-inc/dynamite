using System;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using Microsoft.SharePoint.Publishing;
using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a TaxonomyMulti field
    /// </summary>
    public class TaxonomyMultiFieldInfo : FieldInfo<TaxonomyValueCollection>
    {
        /// <summary>
        /// Initializes a new FieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        public TaxonomyMultiFieldInfo(string internalName, Guid id)
            : base(internalName, id, "TaxonomyFieldTypeMulti")
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is open]; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; set; }

        /// <summary>
        /// The XML schema of the Taxonomy field
        /// </summary>
        public override XElement Schema
        {
            get
            {
                XNamespace p4 = "http://www.w3.org/2001/XMLSchema-instance";
                var schema = new XElement(
                    "Field",
                    new XAttribute("Name", this.InternalName),
                    new XAttribute("Type", this.Type),
                    new XAttribute("ID", "{" + this.Id + "}"),
                    new XAttribute("StaticName", this.InternalName),
                    new XAttribute("DisplayName", this.DisplayName),
                    new XAttribute("Description", this.Description),
                    new XAttribute("Group", this.Group),
                    new XAttribute("EnforceUniqueValues", this.EnforceUniqueValues.ToString().ToUpper()),
                    new XAttribute("Mult", "TRUE"),
                    new XElement(
                        "Customization",
                        new XElement(
                            "ArrayOfProperty",
                            new XElement(
                                "Property",
                                new XElement("Name", "TextField"),
                                new XElement(
                                    "Value",
                                    new XAttribute(XNamespace.Xmlns + "q6", "http://www.w3.org/2001/XMLSchema"),
                                    new XAttribute(p4 + "type", "q6:string"),
                                    new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                                    "{" + Guid.NewGuid() + "}")),
                            new XElement(
                                "Property",
                                new XElement("Name", "IsPathRendered"),
                                new XElement(
                                    "Value",
                                    new XAttribute(XNamespace.Xmlns + "q7", "http://www.w3.org/2001/XMLSchema"),
                                    new XAttribute(p4 + "type", "q7:boolean"),
                                    new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                                    "false")))));

                // Check the Required type
                if (this.Required == RequiredTypes.Required)
                {
                    schema.Add(new XAttribute("Required", "TRUE"));
                }

                if (this.Required == RequiredTypes.NotRequired)
                {
                    schema.Add(new XAttribute("Required", "FALSE"));
                }

                return schema;
            }
        }
    }
}
