using System;
using GSoft.Dynamite.Globalization.Variations;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Catalog navigation interface.
    /// </summary>
    public interface ICatalogNavigation
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        CatalogNavigationType Type { get; }

        /// <summary>
        /// Gets the variation peer URL.
        /// </summary>
        /// <param name="label">The variation label.</param>
        /// <returns>The peer URL.</returns>
        Uri GetVariationPeerUrl(VariationLabel label);

        /// <summary>
        /// Gets the variation peer URL.
        /// </summary>
        /// <param name="label">The variation label (cacheable object).</param>
        /// <returns>The peer URL.</returns>
        Uri GetVariationPeerUrl(VariationLabelInfo label);

        /// <summary>
        /// Gets the variation peer URL.
        /// </summary>
        /// <param name="label">The variation label.</param>
        /// <param name="associationKeyManagedPropertyName">Managed property name for association key between variation peer items</param>
        /// <param name="associationKeyValue">Value of the association key for the current item under variation</param>
        /// <param name="languageManagedPropertyName">Managed property name for the language discriminator column</param>
        /// <param name="catalogNavigationTermManagedPropertyName">Managed property name for the catalog navigation taxonomy column</param>
        /// <returns>
        /// The peer URL.
        /// </returns>
        Uri GetVariationPeerUrlForCatalogItem(
            VariationLabelInfo label,
            string associationKeyManagedPropertyName,
            string associationKeyValue,
            string languageManagedPropertyName,
            string catalogNavigationTermManagedPropertyName);

        /// <summary>
        /// Determines whether [is current item] [the specified item URL].
        /// </summary>
        /// <param name="itemUrl">The item URL.</param>
        /// <returns>True if URL is the current catalog item.</returns>
        bool IsCurrentItem(Uri itemUrl);
    }
}
