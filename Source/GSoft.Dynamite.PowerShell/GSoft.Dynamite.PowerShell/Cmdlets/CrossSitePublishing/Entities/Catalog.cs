using System.Xml.Serialization;

namespace GSoft.Dynamite.PowerShell.Cmdlets.CrossSitePublishing.Entities
{
    /// <summary>
    /// Catalog configuration.
    /// </summary>
    public class Catalog
    {
        /// <summary>
        /// Gets or sets the root folder URL.
        /// </summary>
        /// <value>
        /// The root folder URL.
        /// </value>
        [XmlAttribute]
        public string RootFolderUrl { get; set; }

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
        /// Gets or sets the list template identifier.
        /// </summary>
        /// <value>
        /// The list template identifier.
        /// </value>
        [XmlAttribute]
        public int ListTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the taxonomy field map.
        /// </summary>
        /// <value>
        /// The taxonomy field map.
        /// </value>
        [XmlAttribute]
        public string TaxonomyFieldMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [overwrite].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [remove default content type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [remove default content type]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool RemoveDefaultContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [has draft visibility type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [has draft visibility type]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool HasDraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the draft visibility.
        /// </summary>
        /// <value>
        /// The type of the draft visibility.
        /// </value>
        [XmlAttribute]
        public string DraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable ratings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable ratings]; otherwise, <c>false</c>.
        /// </value>
        [XmlAttribute]
        public bool EnableRatings { get; set; }

        /// <summary>
        /// Gets or sets the type of the rating.
        /// </summary>
        /// <value>
        /// The type of the rating.
        /// </value>
        [XmlAttribute]
        public string RatingType { get; set; }

        /// <summary>
        /// Gets or sets the write security.
        /// 1 — All users can modify all items.
        /// 2 — Users can modify only items that they create.
        /// 4 — Users cannot modify any list item.
        /// </summary>
        /// <value>
        /// The write security.
        /// </value>
        [XmlAttribute]
        public int WriteSecurity { get; set; }

        /// <summary>
        /// Gets or sets the content types.
        /// </summary>
        /// <value>
        /// The content types.
        /// </value>
        [XmlArray]
        public ContentType[] ContentTypes { get; set; }

        /// <summary>
        /// Gets or sets the managed properties.
        /// </summary>
        /// <value>
        /// The managed properties.
        /// </value>
        [XmlArray, XmlArrayItem("Property")]
        public ManagedProperty[] ManagedProperties { get; set; }

        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        [XmlArray, XmlArrayItem(Type = typeof(TextField)), XmlArrayItem(Type = typeof(TaxonomyField))]
        public BaseField[] Segments { get; set; }

        /// <summary>
        /// Gets or sets the default values.
        /// </summary>
        /// <value>
        /// The default values.
        /// </value>
        [XmlArray, XmlArrayItem(Type = typeof(TextField)), XmlArrayItem(Type = typeof(TaxonomyField))]
        public BaseField[] Defaults { get; set; }

        /// <summary>
        /// Gets or sets the field display settings.
        /// </summary>
        /// <value>
        /// The field display settings.
        /// </value>
        [XmlArray, XmlArrayItem("Field")]
        public BaseField[] FieldDisplaySettings { get; set; }
    }
}
