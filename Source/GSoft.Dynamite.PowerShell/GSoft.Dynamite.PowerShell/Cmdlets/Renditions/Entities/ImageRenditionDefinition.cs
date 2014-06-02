using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.Renditions.Entities
{
    /// <summary>
    /// Image existingImageRendition.
    /// </summary>
    [XmlRoot("ImageRendition")]
    public class ImageRenditionDefinition
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
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        [XmlAttribute]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        [XmlAttribute]
        public int Height { get; set; }
    }
}
