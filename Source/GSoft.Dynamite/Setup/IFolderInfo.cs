namespace GSoft.Dynamite.Setup
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.SharePoint;
    using System;

    /// <summary>
    /// Metadata about a folder inside the Pages library
    /// </summary>
    [Obsolete]
    public interface IFolderInfo
    {
        /// <summary>
        /// Label for the folder - will appear in url
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Subfolder of this folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<IFolderInfo> Subfolders { get; set; }

        /// <summary>
        /// A set of field default for the folder's sub items
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<IFieldValueInfo> Defaults { get; set; }

        /// <summary>
        /// A set of field values for the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<IFieldValueInfo> Values { get; set; }

        /// <summary>
        /// Items (their field values) that should be added to the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<IPageInfo> Pages { get; set; }

        /// <summary>
        /// Gets or sets the unique content type order.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        IList<SPContentTypeId> UniqueContentTypeOrder { get; set; }
    }
}