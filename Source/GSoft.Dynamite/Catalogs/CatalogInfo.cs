using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Search;

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
            this.IsSynced = true;
            this.ManagedProperties = new List<ManagedPropertyInfo>();
        }

        /// <summary>
        /// Initializes a new CatalogInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        public CatalogInfo(Uri webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : base(webRelativeUrl, displayNameResourceKey, descriptionResourceKey)
        {
            this.IsAnonymous = false;
            this.IsSynced = true;
            this.ManagedProperties = new List<ManagedPropertyInfo>();
        }

        /// <summary>
        /// Initializes a new CatalogInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        public CatalogInfo(string webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : this(new Uri(webRelativeUrl, UriKind.Relative), displayNameResourceKey, descriptionResourceKey)
        {
        }

        /// <summary>
        /// Taxonomy field used for navigation
        /// </summary>
        public TaxonomyFieldInfo TaxonomyFieldMap { get; set; }

        /// <summary>
        /// Managed properties exposed through the catalog
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow replacement of backing store for more flexible intialization of collection.")]
        public ICollection<ManagedPropertyInfo> ManagedProperties { get; set; }

        /// <summary>
        /// Specifies if the catalog must be anonymous
        /// </summary>
        public bool IsAnonymous { get; set; }
    }
}
