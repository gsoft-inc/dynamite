namespace GSoft.Dynamite.Globalization.Variations
{
    using System.Collections.ObjectModel;

    using Microsoft.SharePoint;

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
        /// Sync a SPList for a target label
        /// </summary>
        /// <param name="listToSync">The source SPList instance to sync.</param>
        /// <param name="labelToSync">The label name to Sync. example: <c>"en"</c> or <c>"fr"</c>.</param>
        void SyncList(SPList listToSync, string labelToSync);

        /// <summary>
        /// Sync a SPWeb with variations
        /// </summary>
        /// <param name="web">The source web instance to sync.</param>
        /// <param name="labelToSync">Source label to sync</param>
        void SyncWeb(SPWeb web, string labelToSync);
    }
}