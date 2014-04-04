using System;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Caching.Entities
{
    /// <summary>
    /// Variation label class that is serializable for caching purposes.
    /// </summary>
    [Serializable]
    public class CacheVariationLabel : ICacheVariationLabel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheVariationLabel"/> class.
        /// </summary>
        /// <param name="variationLabel">The variation label.</param>
        public CacheVariationLabel(VariationLabel variationLabel)
        {
            this.DisplayName = variationLabel.DisplayName;
            this.IsSource = variationLabel.IsSource;
            this.Language = variationLabel.Language;
            this.Locale = variationLabel.Locale;
            this.Title = variationLabel.Title;
            this.TopWebUrl = new Uri(variationLabel.TopWebUrl);
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether [is source].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is source]; otherwise, <c>false</c>.
        /// </value>
        public bool IsSource { get; set; }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string Language { get; set; }

        /// <summary>
        /// Gets the locale.
        /// </summary>
        /// <value>
        /// The locale.
        /// </value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets the top web URL.
        /// </summary>
        /// <value>
        /// The top web URL.
        /// </value>
        public Uri TopWebUrl { get; set; }
    }
}
