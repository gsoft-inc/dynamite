using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.UserProfile.Entities
{
    /// <summary>
    /// User profile property.
    /// </summary>
    [XmlRoot("UserProperty")]
    public class UserProfileProperty
    {
        /// <summary>
        /// Gets or sets the general settings.
        /// </summary>
        /// <value>
        /// The general settings.
        /// </value>
        public GeneralSettings GeneralSettings { get; set; }

        /// <summary>
        /// Gets or sets the display settings.
        /// </summary>
        /// <value>
        /// The display settings.
        /// </value>
        public DisplaySettings DisplaySettings { get; set; }
    }
}
