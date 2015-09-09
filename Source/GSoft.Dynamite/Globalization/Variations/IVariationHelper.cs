using System;
using System.Collections.ObjectModel;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// SharePoint variations helper
    /// </summary>
    public interface IVariationHelper
    {
        /// <summary>
        /// Determines whether [the specified web] [is current web source label].
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns>A boolean value which indicates if the current web is the source variation label.</returns>
        bool IsCurrentWebSourceLabel(SPWeb web);

        /// <summary>
        /// Determines if variations are enabled on a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A boolean value which indicates if the current site has variations enabled.</returns>
        bool IsVariationsEnabled(SPSite site);

        /// <summary>
        /// Get the variations labels for the site collection.
        /// NOTE: Also possible with the static Microsoft.SharePoint.Publishing Variations object by faking a SPContext
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="labelToSync">The label name to Sync. example: <c>"en"</c> or <c>"fr"</c>.</param>
        /// <returns>A collection of unique label.</returns>
        ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(SPSite site, string labelToSync);

        /// <summary>
        /// Get the variations labels for the site collection.
        /// NOTE: Also possible with the static Microsoft.SharePoint.Publishing Variations object by faking a SPContext
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A collection of unique label.</returns>
        ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(SPSite site);

        /// <summary>
        /// Get the variations labels using current the current SPContext
        /// </summary>
        /// <param name="currentUrl">The current url context</param>
        /// <returns>A collection of unique label.</returns>
        /// <param name="excludeCurrentLabel">True to exclude the current context label. False otherwise</param>
        ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(Uri currentUrl, bool excludeCurrentLabel);

        /// <summary>
        /// Setup variations on a site
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="variationSettings">The variation settings</param>
        void SetupVariations(SPSite site, VariationSettingsInfo variationSettings);

        /// <summary>
        /// Get the hidden relationships list for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The relationships list.</returns>
        SPList GetVariationLabelHiddenList(SPSite site);

        /// <summary>
        /// Get the hidden variation labels for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>the variation labels list.</returns>
        SPList GetRelationshipsHiddenList(SPSite site);
    }
}