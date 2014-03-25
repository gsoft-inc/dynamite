using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.UserProfile.Entities
{
    /// <summary>
    /// User profile property display settings
    /// </summary>
    public class DisplaySettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether [is event log].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is event log]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsEventLog { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible on editor].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible on editor]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsVisibleOnEditor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible on viewer].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible on viewer]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsVisibleOnViewer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is user editable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is user editable]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool IsUserEditable { get; set; }

        /// <summary>
        /// Gets or sets the privacy.
        /// </summary>
        /// <value>
        /// The privacy.
        /// </value>
        [XmlAttribute]
        public string Privacy { get; set; }

        /// <summary>
        /// Gets or sets the privacy policy.
        /// </summary>
        /// <value>
        /// The privacy policy.
        /// </value>
        [XmlAttribute]
        public string PrivacyPolicy { get; set; }
    }
}
