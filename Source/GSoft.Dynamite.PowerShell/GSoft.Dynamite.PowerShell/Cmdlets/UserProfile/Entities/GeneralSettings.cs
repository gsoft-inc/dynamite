using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.UserProfile.Entities
{
    /// <summary>
    /// User profile general settings
    /// </summary>
    public class GeneralSettings
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        [XmlAttribute]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [XmlAttribute]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is alias].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is alias]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is multi valued].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is multi valued]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsMultiValued { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is searchable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is searchable]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsSearchable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is section].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is section]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsSection { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        [XmlAttribute]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [XmlAttribute]
        public int Order { get; set; }
    }
}
