using System.Collections.Generic;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a catalog
    /// </summary>
    public class CatalogInfo : ListInfo
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public CatalogInfo()
        {
            this.IsAnonymous = false;
        }

        /// <summary>
        /// Taxonomy field used for navigation
        /// </summary>
        public TaxonomyFieldInfo TaxonomyFieldMap { get; set; }

        /// <summary>
        /// Managed properties exposed through the catalog
        /// </summary>
        public IList<ManagedPropertyInfo> ManagedProperties { get; set; }

        /// <summary>
        /// Enforce unique values on the navigation column
        /// </summary>
        public bool EnforceUniqueNavigationValues { get; set; }

        /// <summary>
        /// Specifies if the catalog must be anonymous
        /// </summary>
        public bool IsAnonymous { get; set; }
    }
}
