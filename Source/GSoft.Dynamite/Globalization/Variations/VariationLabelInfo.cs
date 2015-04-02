using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.SharePoint.Publishing;

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
        /// Empty constructor for serialization purposes
        /// </summary>
        public VariationLabelInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheVariationLabel"/> class.
        /// </summary>
        /// <param name="variationLabel">The variation label.</param>
        public VariationLabelInfo(VariationLabel variationLabel)
        {
            this.FlagControlDisplayName = variationLabel.DisplayName;
            this.IsSource = variationLabel.IsSource;
            this.Language = variationLabel.Language;
            this.Locale = TryParse(variationLabel);
            this.Title = variationLabel.Title;
            this.TopWebUrl = new Uri(variationLabel.TopWebUrl);
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the flag control display name.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag", Justification = "This is the property name SharePoint uses for a Variation label's display name.")]
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

        /// <summary>
        /// URL of the top PublishingWeb in the variations hierarchy
        /// </summary>
        public Uri TopWebUrl { get; set; }

        /// <summary>
        /// Gets or sets the custom title value. This value will be displayed instead of the default title value of the variation label. 
        /// If the property is not set, the default value of the variation label is displayed.
        /// </summary>
        public string CustomTitleValue { get; set; }

        /// <summary>
        /// Gets or Sets the CssClass property. Add a css class to the label.
        /// </summary>
        public string CssClass { get; set; }

        private static int TryParse(VariationLabel variationLabel)
        {
            int number;
            var result = int.TryParse(variationLabel.Locale, out number);
            // If the locale is not parsable, we use the Language property.
            if (result)
            {
                return number;
            }
            else
            {
                return new CultureInfo(variationLabel.Language).LCID;
            }
        }
    }
}
