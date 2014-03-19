using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Base field definition.
    /// </summary>
    public abstract class BaseField
    {
        /// <summary>
        /// Gets or sets the name of the internal.
        /// </summary>
        /// <value>
        /// The name of the internal.
        /// </value>
        [XmlAttribute]
        public string InternalName { get; set; }

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
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute("Required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [XmlAttribute]
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in display form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in display form]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInDisplayForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in edit form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in edit form]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInEditForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in list settings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in list settings]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInListSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in new form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in new form]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInNewForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in version history].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show in version history]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInVersionHistory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in view form].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in view form]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool ShowInViewForm { get; set; }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        [XmlElement("Value")]
        public string[] Values { get; set; }
    }
}
