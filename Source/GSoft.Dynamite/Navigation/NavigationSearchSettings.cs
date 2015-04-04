using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Navigation search related settings.
    /// </summary>
    public class NavigationSearchSettings
    {
        /// <summary>
        /// The navigation filter managed property name
        /// </summary>
        public string NavigationManagedPropertyName { get; set; }

        /// <summary>
        /// The result source name
        /// </summary>
        public string ResultSourceName { get; set; }

        /// <summary>
        /// The list of selected properties from the search query.
        /// See https://msdn.microsoft.com/en-us/library/microsoft.sharepoint.client.search.query.keywordquery.selectproperties.aspx
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage", 
            "CA2227:CollectionPropertiesShouldBeReadOnly", 
            Justification = "Not making this property read-only helps facilitate setting the value.")]
        public ICollection<string> SelectedProperties { get; set; }

        /// <summary>
        /// Gets the filters to apply to the all search queries.
        /// ex: MyManagedPropertyOWSTEXT:myvalue
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Not making this property read-only helps facilitate setting the value.")]
        public ICollection<string> GlobalFilters { get; set; }

        /// <summary>
        /// Gets the filters to apply to the search queries related to target items.
        /// ex: ContentTypeId:0x01000210210*
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Not making this property read-only helps facilitate setting the value.")]
        public ICollection<string> TargetItemFilters { get; set; }

        /// <summary>
        /// Gets the filters to apply to the search queries related to catalog items.
        /// ex: ContentTypeId:0x010002102101*
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Not making this property read-only helps facilitate setting the value.")]
        public ICollection<string> CatalogItemFilters { get; set; }
    }
}