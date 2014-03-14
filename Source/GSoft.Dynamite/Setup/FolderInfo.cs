using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Setup;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Data used in creating translatable folders by <see cref="FolderMaker"/>
    /// </summary>
    public class FolderInfo : IFolderInfo
    {
        private IList<IFolderInfo> subFolders;
        private IList<IFieldValueInfo> defaults;
        private IList<IFieldValueInfo> values;
        private IList<IPageInfo> pages;
        private IList<SPContentTypeId> uniqueContentTypeOrder;

        /// <summary>
        /// Label for the folder - will appear in url
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Subfolder of this folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<IFolderInfo> Subfolders 
        {
            get
            {
                if (this.subFolders == null)
                {
                    this.subFolders = new List<IFolderInfo>();
                }

                return this.subFolders;
            }

            set
            {
                this.subFolders = value;
            }
        }

        /// <summary>
        /// A set of field default for the folder's sub items
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<IFieldValueInfo> Defaults 
        {
            get
            {
                if (this.defaults == null)
                {
                    this.defaults = new List<IFieldValueInfo>();
                }

                return this.defaults;
            }

            set
            {
                this.defaults = value;
            } 
        }

        /// <summary>
        /// A set of field values for the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<IFieldValueInfo> Values 
        {
            get
            {
                if (this.values == null)
                {
                    this.values = new List<IFieldValueInfo>();
                }

                return this.values;
            }

            set
            {
                this.values = value;
            }
        }

        /// <summary>
        /// Items (their field values) that should be added to the folder
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<IPageInfo> Pages 
        {
            get
            {
                if (this.pages == null)
                {
                    this.pages = new List<IPageInfo>();
                }

                return this.pages;
            }

            set
            {
                this.pages = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique content type order.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Easier folder tree initialization with setter")]
        public IList<SPContentTypeId> UniqueContentTypeOrder 
        {
            get
            {
                if (this.uniqueContentTypeOrder == null)
                {
                    this.uniqueContentTypeOrder = new List<SPContentTypeId>();
                }

                return this.uniqueContentTypeOrder;
            }

            set
            {
                this.uniqueContentTypeOrder = value;
            }
        }
    }
}
