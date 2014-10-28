using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Taxonomy Field schema
    /// </summary>
    public class TaxonomyFieldSchema : GenericFieldSchema
    {
        private bool isMultiple, enforceUniqueValues;

        #region Properties

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
                return this.isMultiple;
            }

            set
            {
                if (value == false)
                {
                    this.isMultiple = true;
                    this.FieldType = "TaxonomyFieldType";
                }
                else
                {
                    this.FieldType = "TaxonomyFieldTypeMulti";
                }       
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enforce unique values].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enforce unique values]; otherwise, <c>false</c>.
        /// </value>
        public bool EnforceUniqueValues
        {
            get { return this.enforceUniqueValues; }
            set { this.enforceUniqueValues = value; }
        }

        #endregion

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public override XElement ToXElement()
        {
            XNamespace p4 = "http://www.w3.org/2001/XMLSchema-instance";
            this.FieldSchema = new XElement(
                "Field",
                new XAttribute("Name", this.FieldName),
                new XAttribute("Type", this.FieldType),
                new XAttribute("ID", "{" + this.FieldId + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup),
                new XAttribute("EnforceUniqueValues", this.enforceUniqueValues.ToString().ToUpper()),
                new XAttribute("Mult", this.isMultiple.ToString().ToUpper()),
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
