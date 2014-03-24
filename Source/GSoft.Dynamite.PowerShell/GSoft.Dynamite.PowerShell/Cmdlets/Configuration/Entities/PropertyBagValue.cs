using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Configuration.Entities
{
    /// <summary>
    /// A property bag value
    /// </summary>
    public class PropertyBagValue
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        [XmlAttribute]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [XmlAttribute]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [overwrite].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [indexed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [indexed]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool Indexed { get; set; }
    }
}
