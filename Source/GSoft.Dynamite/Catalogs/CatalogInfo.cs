using GSoft.Dynamite.FieldTypes;
using System.Collections.Generic;
using GSoft.Dynamite.Definitions;

namespace GSoft.Dynamite.Catalogs
{
    /// <summary>
    /// Definition for a catalog
    /// </summary>
    public class CatalogInfo : ListInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public CatalogInfo()
        {
            this.IsAnonymous = false;
            this.ManagedProperties = new List<ManagedPropertyInfo>();
        }
        
        /// <summary>
        /// Initializes a new CatalogInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        public CatalogInfo(string webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : base(webRelativeUrl, displayNameResourceKey, descriptionResourceKey)
        {
            this.IsAnonymous = false;
            this.ManagedProperties = new List<ManagedPropertyInfo>();
        }

        /// <summary>
        /// Taxonomy field used for navigation
        /// </summary>
        public TaxonomyFieldInfo TaxonomyFieldMap { get; set; }

        /// <summary>
        /// Managed properties exposed through the catalog
        /// </summary>
        public ICollection<ManagedPropertyInfo> ManagedProperties { get; set; }

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
