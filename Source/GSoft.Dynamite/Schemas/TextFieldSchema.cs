using System.Xml.Linq;

namespace GSoft.Dynamite.Schemas
{
    /// <summary>
    /// Text field schema.
    /// </summary>
    public class TextFieldSchema : GenericFieldSchema
    {
        private bool _isMultiLine;
        
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [is multi line].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multi line]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiLine
        {
            get
            {
                return this._isMultiLine;
            }

            set
            {
                if (value == false)
                {
                    this._isMultiLine = true;
                    this.FieldType = "Text";
                }
                else
                {
                    this.FieldType = "Note";
                }
            }
        }

        #endregion

        /// <summary>
        /// Get the XML schema of the field.
        /// </summary>
        /// <returns>The XML schema.</returns>
        public override XElement ToXElement()
        {
            this.FieldSchema = new XElement(
                "Field",
                new XAttribute("Name", this.FieldName),
                new XAttribute("Type", this.FieldType),
                new XAttribute("ID", "{" + this.FieldId + "}"),
                new XAttribute("StaticName", this.FieldStaticName),
                new XAttribute("DisplayName", this.FieldDisplayName),
                new XAttribute("Description", this.FieldDescription),
                new XAttribute("Group", this.FieldGroup),
                new XAttribute("ShowInListSettings", "TRUE"));

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
