using System;
using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Definitions.Values;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a Taxonomy field
    /// </summary>
    public class TaxonomyFieldInfo : FieldInfo
    {
        private bool _isMultiple;

        /// <summary>
        /// Default constructor
        /// </summary>
        public TaxonomyFieldInfo()
        {
            // Default Taxonomy Type
            this.Type = "TaxonomyFieldType";
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiple
        {
            get
            {
                return this._isMultiple;
            }

            set
            {
                if (value == false)
                {
                    this._isMultiple = true;
                    this.Type = "TaxonomyFieldType";
                }
                else
                {
                    this.Type = "TaxonomyFieldTypeMulti";
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is open].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is open]; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen { get; set; }

        /// <summary>
        /// TThe XML schema of a Taxonomy field as XElement
        /// </summary>
        /// <returns>The XML schema of a Taxonomy field as XElement</returns>
        public override XElement ToXElement()
        {
            XNamespace p4 = "http://www.w3.org/2001/XMLSchema-instance";
            this.Schema = new XElement(
                "Field",
                new XAttribute("Name", this.InternalName),
                new XAttribute("Type", this.Type),
                new XAttribute("ID", "{" + this.Id + "}"),
                new XAttribute("StaticName", this.StaticName),
                new XAttribute("DisplayName", this.DisplayName),
                new XAttribute("Description", this.Description),
                new XAttribute("Group", this.Group),
                new XAttribute("EnforceUniqueValues", this.EnforceUniqueValues.ToString().ToUpper()),
                new XAttribute("Mult", this.IsMultiple.ToString().ToUpper()),
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
            if (this.RequiredType == RequiredTypes.Required)
            {
                this.Schema.Add(new XAttribute("Required", "TRUE"));
            }

            if (this.RequiredType == RequiredTypes.NotRequired)
            {
                this.Schema.Add(new XAttribute("Required", "FALSE"));
            }

            return this.Schema;
        }
    }
}
