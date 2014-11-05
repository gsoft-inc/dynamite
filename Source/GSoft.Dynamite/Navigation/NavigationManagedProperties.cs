using Microsoft.SharePoint.Publishing;
using System.Collections.Generic;
namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Managed property names
    /// </summary>
    public class NavigationManagedProperties
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        public NavigationManagedProperties()
        {
        }

        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The item language
        /// </summary>
        public string ItemLanguage { get; set; }

        /// <summary>
        /// The navigation managed property name
        /// </summary>
        public string Navigation { get; set; }

        /// <summary>
        /// The friendly URL required properties
        /// </summary>
        public string[] FriendlyUrlRequiredProperties { get; set; }

        /// <summary>
        /// The result source name
        /// </summary>
        public string ResultSourceName { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        public string CatalogItemId { get; set; }

        /// <summary>
        /// The list of query properties 
        /// </summary>
        public IList<string> queryProperties { get; set; }
    }
}
