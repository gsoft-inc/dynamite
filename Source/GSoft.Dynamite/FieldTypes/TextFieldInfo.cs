using System.Xml.Linq;
using GSoft.Dynamite.Binding;
using System;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a TextField info
    /// </summary>
    public class TextFieldInfo : FieldInfo<string>
    {
        private bool _isMultiLine;

        /// <summary>
        /// Initializes a new TextFieldInfo
        /// </summary>
        /// <param name="internalName">The internal name of the field</param>
        /// <param name="id">The field identifier</param>
        public TextFieldInfo(string internalName, Guid id) : base(internalName, id)
        {
            // Default Text Field Type
            this.Type = "Text";
        }

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
                    this.Type = "Text";
                }
                else
                {
                    this.Type = "Note";
                }
            }
        }

        #endregion

        /// <summary>
        /// The XML schema of a Text field as XElement
        /// </summary>
        /// <returns>The XML schema of a Text field as XElement</returns>
        public override XElement ToXElement()
        {

            this.Schema = new XElement(
                                        "Field",
                                        new XAttribute("Name", this.InternalName),
                                        new XAttribute("Type", this.Type),
                                        new XAttribute("ID", "{" + this.Id + "}"),
                                        new XAttribute("StaticName", this.StaticName),
                                        new XAttribute("DisplayName", this.DisplayName),
                                        new XAttribute("Description", this.Description),
                                        new XAttribute("Group", this.Group),
                                        new XAttribute("ShowInListSettings", "TRUE"));

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
