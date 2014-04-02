using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Metadata for a pair of translatable pages
    /// </summary>
    public class PageInfo : IPageInfo
    {
        /// <summary>
        /// English label for the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The page layout's content type id
        /// </summary>
        public SPContentTypeId ContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the page layout.
        /// Overrides the page layout found by content type Id.
        /// </summary>
        /// <value>
        /// The page layout.
        /// </value>
        public PageLayout PageLayout { get; set; }

        /// <summary>
        /// Whether the item should be flagged as the Welcome Page of its parent folder
        /// </summary>
        public bool IsWelcomePage { get; set; }

        /// <summary>
        /// A set of field values for the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<IFieldValueInfo> Values { get; set; }
    }
}
