using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GSoft.Dynamite.Definitions
{
    public class TextFieldInfo: FieldInfo
    {
        private bool _isMultiLine;

        public TextFieldInfo()
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

        public override System.Xml.Linq.XElement ToXElement()
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

            return this.Schema;
        }
    }
}
