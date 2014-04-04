using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Managed property definition.
    /// </summary>
    public class ManagedProperty
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }
    }
}
