using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Content type definition.
    /// </summary>
    public class ContentType
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [XmlAttribute(AttributeName = "ID")]
        public string Id { get; set; }
    }
}
