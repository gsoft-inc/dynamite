using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Text field definition.
    /// </summary>
    public class TextField : BaseField
    {
        /// <summary>
        /// Gets or sets a value indicating whether [is multiline].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multiline]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsMultiline { get; set; }
    }
}
