using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    public class TextFieldSchema: GenericFieldSchema
    {
        private bool _isMultiLine = false;
        
        #region Properties

        public bool IsMultiLine
        {
            get { return _isMultiLine; }
            set
            {
                if (value == false)
                {
                    this._isMultiLine = true;
                    this._fieldType = "Text";
                }
                else
                {
                    this._fieldType = "Note";
                }
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextFieldSchema() : base() { }

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public override XElement ToXElement()
        {

            this._fieldSchema =
             new XElement("Field",
                new XAttribute("Name", this.FieldName),
                new XAttribute("Type", this.FieldType),
                new XAttribute("ID", "{" + this._fieldId.ToString() + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup));

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
