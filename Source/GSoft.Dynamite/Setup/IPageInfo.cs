namespace GSoft.Dynamite.Setup
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    /// <summary>
    /// Metadata about a publishing page
    /// </summary>
    public interface IPageInfo
    {
        /// <summary>
        /// English label for the item
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The page layout's content type id
        /// </summary>
        SPContentTypeId ContentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the page layout.
        /// Overrides the index-0 page layout found by content type Id.
        /// </summary>
        /// <value>
        /// The page layout.
        /// </value>
        PageLayout PageLayout { get; set; }

        /// <summary>
        /// Whether the item should be flagged as the Welcome Page of its parent folder
        /// </summary>
        bool IsWelcomePage { get; set; }

        /// <summary>
        /// A set of field values for the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<IFieldValueInfo> Values { get; set; }
    }
}