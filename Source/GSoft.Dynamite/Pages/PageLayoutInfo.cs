using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Definition of a page layout info
    /// </summary>
    public class PageLayoutInfo
    {
        /// <summary>
        /// Initializes a new <see cref="PageLayoutInfo"/> instance
        /// </summary>
        public PageLayoutInfo()
        {
            this.ZoneNames = new List<string>();
        }

        /// <summary>
        /// Initializes a new <see cref="PageLayoutInfo"/> instance
        /// </summary>
        /// <param name="name">Page layout name</param>
        /// <param name="associatedContentTypeId">Associated page content type ID</param>
        public PageLayoutInfo(string name, string associatedContentTypeId) : this()
        {
            this.Name = name;
            this.AssociatedContentTypeId = associatedContentTypeId;
        }

        /// <summary>
        /// Name of the Page Layout
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Names of the zones in the page layout
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow the replacement of the backing collection store for more flexible initialization.")]
        public ICollection<string> ZoneNames { get; set; }

        /// <summary>
        /// The associated content type id
        /// </summary>
        public string AssociatedContentTypeId { get; set; }

        /// <summary>
        /// The relative path of the preview picture of the page layout
        /// </summary>
        public string PreviewImagePath { get; set; }
    }
}
