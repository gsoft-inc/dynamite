using System;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Taxonomy Field schema
    /// </summary>
    public class TaxonomyFieldSchema : GenericFieldSchema
    {
        private bool _isMultiple = false;
        private bool _enforceUniqueValues = false;

        #region Properties

        public bool IsMultiple
        {
            get { return _isMultiple; }
            set { 
                if (value == false)
                {
                    this._isMultiple = true;
                    this._fieldType = "TaxonomyFieldType";
                }
                else
                {
                    this._fieldType = "TaxonomyFieldTypeMulti";
                }       
            }
        }

        public bool EnforceUniqueValues
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
                new XAttribute("ID", "{" + this._fieldId.ToString() + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup),
                new XAttribute("EnforceUniqueValues", this._enforceUniqueValues.ToString().ToUpper()),
                new XAttribute("Mult", this._isMultiple.ToString().ToUpper()),
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
