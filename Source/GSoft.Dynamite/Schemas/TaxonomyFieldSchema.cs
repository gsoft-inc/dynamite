using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Taxonomy Field schema
    /// </summary>
    public class TaxonomyFieldSchema : GenericFieldSchema
    {
        private string _isMultiple = "FALSE";
        private string _enforceUniqueValues = "FALSE";

        #region Properties

        public string IsMultiple
        {
            get { return _isMultiple; }
            set { _isMultiple = value; }
        }

        public string EnforceUniqueValues
        {
            get { return _enforceUniqueValues; }
            set { _enforceUniqueValues = value; }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public TaxonomyFieldSchema() : base() { }

        /// <summary>
        /// Inititlaize a Taxonomy Field Schema 
        /// </summary>
        /// <param name="isMultiple">Specifies if the field allows multiple values.</param>
        public TaxonomyFieldSchema(bool isMultiple)
        {
            if (isMultiple)
            {
                this._isMultiple = "TRUE";
                this._fieldType = "TaxonomyFieldType";
            }
            else
            {
                this._fieldType = "TaxonomyFieldTypeMulti";
            }
        }

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public override XElement ToXElement()
        {

            XNamespace xmlns = "http://schemas.microsoft.com/sharepoint/";
            XNamespace p4 = "http://www.w3.org/2001/XMLSchema-instance";

            this._fieldSchema =
             new XElement("Field",
                new XAttribute("Name", this.FieldName),
                new XAttribute("Type", this.FieldType),
                new XAttribute("ID", "{" + Guid.NewGuid().ToString() + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup),
                new XAttribute("EnforceUniqueValues", this._enforceUniqueValues),
                new XAttribute("Mult", this._isMultiple),
                new XElement("Customization",
                    new XElement("ArrayOfProperty",
                        new XElement("Property",
                            new XElement("Name", "TextField"),
                            new XElement("Value",
                                new XAttribute(XNamespace.Xmlns +"q6", "http://www.w3.org/2001/XMLSchema"),
                                new XAttribute(p4+"type", "q6:string"),
                                new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                                "{" + Guid.NewGuid().ToString() + "}")),
                        new XElement("Property",
                            new XElement("Name", "IsPathRendered"),
                            new XElement("Value",
                                new XAttribute(XNamespace.Xmlns + "q7", "http://www.w3.org/2001/XMLSchema"),
                                new XAttribute(p4+"type", "q7:boolean"),
                                new XAttribute(XNamespace.Xmlns + "p4", "http://www.w3.org/2001/XMLSchema-instance"),
                                "false")
                                )
                            )
                        )
                    );

            return _fieldSchema;
        }

        /// <summary>
        /// Get the XML schema as string of the field.
        /// </summary>
        /// <returns>A string that represents the XML schema.</returns>
        public override string ToString()
        {
            return this._fieldSchema.ToString();
        }
    }
}
