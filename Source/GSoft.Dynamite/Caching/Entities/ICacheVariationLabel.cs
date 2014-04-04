using System;

namespace GSoft.Dynamite.Caching.Entities
{
    /// <summary>
    /// Cache variation label interface.
    /// </summary>
    [Obsolete]
    public interface ICacheVariationLabel
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets a value indicating whether [is source].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is source]; otherwise, <c>false</c>.
        /// </value>
        bool IsSource { get; set; }

        /// <summary>
        /// Gets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        string Language { get; set; }

        /// <summary>
        /// Gets the locale.
        /// </summary>
        /// <value>
        /// The locale.
        /// </value>
        string Locale { get; set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets the top web URL.
        /// </summary>
        /// <value>
        /// The top web URL.
        /// </value>
        Uri TopWebUrl { get; set; }
    }
}
