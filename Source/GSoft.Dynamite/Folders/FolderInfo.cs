using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.ItemFieldValues = new List<IFieldInfo>();
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
        /// Name of the folder
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Pages in the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<PageInfo> Pages { get; set; }

        /// <summary>
        /// Sub folders
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<FolderInfo> Subfolders { get; set; }

        /// <summary>
        /// Values for the folder should be stored in the DefaultValue
        /// property of the FieldInfo objects.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable more flexible initialization.")]
        public ICollection<IFieldInfo> ItemFieldValues { get; set; }

        /// <summary>
        /// True if the folder is a root folder
        /// </summary>
        public bool IsRootFolder { get; set; }

        /// <summary>
        /// The Welcome Page of the folder
        /// </summary>
        public PageInfo WelcomePage { get; set; }

        /// <summary>
        /// The culture to create that folder hierarchy. If null, create in all culture.
        /// </summary>
        public CultureInfo Locale { get; set; }
    }
}
