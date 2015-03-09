using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Pages;

namespace GSoft.Dynamite.Folders
{
    /// <summary>
    /// Definition of a folder in a library
    /// </summary>
    public class FolderInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public FolderInfo()
        {
            this.Pages = new List<PageInfo>();
            this.Subfolders = new List<FolderInfo>();
            this.FieldDefaultValues = new List<FieldValueInfo>();
            this.UniqueContentTypeOrder = new List<ContentTypeInfo>();
        }

        /// <summary>
        /// Initializes a new <see cref="FolderInfo"/> instance
        /// </summary>
        /// <param name="name">Folder name (path relative to parent)</param>
        public FolderInfo(string name) : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the folder (i.e. its path relative to its parent folder)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Pages in the folder (only supported if folder is ensured within a publishing Pages library)
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<PageInfo> Pages { get; set; }

        /// <summary>
        /// Sub folders
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<FolderInfo> Subfolders { get; set; }

        /// <summary>
        /// Represents the folder's field metadata defaults (per-folder column default values).
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<FieldValueInfo> FieldDefaultValues { get; set; }

        /// <summary>
        /// Determines the list of content types that will be suggested in the folder's "Add Item" dropdown menu (in the SharePoint ribbon).
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<ContentTypeInfo> UniqueContentTypeOrder { get; set; }

        /// <summary>
        /// The Welcome Page of the folder (for automatic redirection when the visitors hit the folder's URL).
        /// When left unspecified, if a page default.aspx exists in the folder, then that default page will be used
        /// as redirection destination.
        /// </summary>
        public PageInfo WelcomePage { get; set; }

        /// <summary>
        /// The specific web culture for which this folder hierarchy was prepared. 
        /// If null, the folder hierarchy should be ensured regardless of the web locale.
        /// </summary>
        public CultureInfo Locale { get; set; }
    }
}
