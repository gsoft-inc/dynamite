using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

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
            this.FriendlyUrlRequiredProperties = new List<string>();
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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow repalcement of backing store for more flexible initialization.")]
        public ICollection<string> FriendlyUrlRequiredProperties { get; set; }

        /// <summary>
        /// The result source name
        /// </summary>
        public string ResultSourceName { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        public SPContentTypeId CatalogItemContentTypeId { get; set; }

        /// <summary>
        /// The Catalog Item Content Type Id 
        /// </summary>
        public SPContentTypeId TargetItemContentTypeId { get; set; }

        /// <summary>
        /// The list of query properties 
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Change 'NavigationManagedProperties.queryProperties' to be read-only by removing the property setter.")]
        public IList<string> QueryProperties { get; set; }

        /// <summary>
        /// The name of a managed property to filter on.
        /// </summary>
        public string FilterManagedPropertyName { get; set; }

        /// <summary>
        /// The value of the managed property to filter on.
        /// </summary>
        public string FilterManagedPropertyValue { get; set; }
    }
}
