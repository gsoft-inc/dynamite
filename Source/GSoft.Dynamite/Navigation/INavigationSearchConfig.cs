using System.Collections.Generic;
using Microsoft.SharePoint;
using Newtonsoft.Json;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation search related configuration.
    /// </summary>
    public interface INavigationSearchConfig
    {
        /// <summary>
        /// The title
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The item language
        /// </summary>
        string ItemLanguage { get; set; }

        /// <summary>
        /// The navigation managed property name
        /// </summary>
        string Navigation { get; set; }

        /// <summary>
        /// The friendly URL required properties
        /// </summary>
        IList<string> FriendlyUrlRequiredProperties { get; }

        /// <summary>
        /// The result source name
        /// </summary>
        string ResultSourceName { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        [JsonIgnore]
        SPContentTypeId CatalogItemContentTypeId { get; set; }

        /// <summary>
        /// String representation of the catalog item content type ID,
        /// convenient for serialization/deserialization.
        /// </summary>
        string CatalogItemContentTypeIdAsString { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        [JsonIgnore]
        SPContentTypeId TargetItemContentTypeId { get; set; }

        /// <summary>
        /// String representation of the target item content type ID,
        /// convenient for serialization/deserialization.
        /// </summary>
        string TargetItemContentTypeIdAsString { get; set; }

        /// <summary>
        /// Gets or sets the filter content type identifier.
        /// </summary>
        /// <value>
        /// The filter content type identifier.
        /// </value>
        [JsonIgnore]
        SPContentTypeId FilterContentTypeId { get; set; }

        /// <summary>
        /// The list of query properties 
        /// </summary>
        IList<string> QueryProperties { get; }

        /// <summary>
        /// The name of a managed property to filter on.
        /// </summary>
        string FilterManagedPropertyName { get; set; }

        /// <summary>
        /// The value of the managed property to filter on.
        /// </summary>
        string FilterManagedPropertyValue { get; set; }
    }
}