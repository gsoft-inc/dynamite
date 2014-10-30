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
        /// Gets or sets the name of the catalog navigation term managed property.
        /// </summary>
        /// <value>
        /// The name of the catalog navigation term managed property.
        /// </value>
        string CatalogNavigationTermManagedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the association key managed property.
        /// </summary>
        /// <value>
        /// The name of the association key managed property.
        /// </value>
        string AssociationKeyManagedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the association key value.
        /// </summary>
        /// <value>
        /// The association key value.
        /// </value>
        string AssociationKeyValue { get; set; }

        /// <summary>
        /// Gets or sets the name of the language managed property.
        /// </summary>
        /// <value>
        /// The name of the language managed property.
        /// </value>
        string LanguageManagedPropertyName { get; set; }

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
        /// Determines whether [is current item] [the specified item URL].
        /// </summary>
        /// <param name="itemUrl">The item URL.</param>
        /// <returns>True if URL is the current catalog item.</returns>
        bool IsCurrentItem(string itemUrl);
    }
}
