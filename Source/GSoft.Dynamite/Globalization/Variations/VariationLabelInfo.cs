namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// The creation mode.
    /// </summary>
    public enum CreationMode
    {
        /// <summary>
        /// The publishing sites and all pages creation mode.
        /// </summary>
        PublishingSitesAndAllPages
    }

    /// <summary>
    /// A simple POCO that represent a variation label
    /// </summary>
    public class VariationLabelInfo
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the flag control display name.
        /// </summary>
        public string FlagControlDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        public int Locale { get; set; }

        /// <summary>
        /// Gets or sets the hierarchy creation mode.
        /// </summary>
        public CreationMode HierarchyCreationMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is source.
        /// </summary>
        public bool IsSource { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }
    }
}
