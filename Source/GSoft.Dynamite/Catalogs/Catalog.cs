using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.SiteColumns;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Catalogs
{
    /// <summary>
    /// Class to create catalogs
    /// </summary>
    public class Catalog
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Catalog()
        {
            // Default value
            this.WriteSecurity = WriteSecurityOptions.AllUser;
        }

        /// <summary>
        /// Gets or sets the root folder URL.
        /// </summary>
        /// <value>
        /// The root folder URL.
        /// </value>
        public string RootFolderUrl { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list template identifier.
        /// </summary>
        /// <value>
        /// The list template identifier.
        /// </value>
        public SPListTemplateType ListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the taxonomy field map.
        /// </summary>
        /// <value>
        /// The taxonomy field map.
        /// </value>
        public string TaxonomyFieldMap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [overwrite].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite]; otherwise, <c>false</c>.
        /// </value>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [remove default content type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [remove default content type]; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveDefaultContentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [has draft visibility type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [has draft visibility type]; otherwise, <c>false</c>.
        /// </value>
        public bool HasDraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the draft visibility.
        /// </summary>
        /// <value>
        /// The type of the draft visibility.
        /// </value>
        public DraftVisibilityType DraftVisibilityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable ratings].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable ratings]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableRatings { get; set; }

        /// <summary>
        /// Gets or sets the type of the rating.
        /// </summary>
        /// <value>
        /// The type of the rating.
        /// </value>
        public string RatingType { get; set; }

        /// <summary>
        /// Gets or sets the write security.
        /// 1 — All users can modify all items.
        /// 2 — Users can modify only items that they create.
        /// 4 — Users cannot modify any list item.
        /// </summary>
        /// <value>
        /// The write security.
        /// </value>
        public WriteSecurityOptions WriteSecurity { get; set; }

        /// <summary>
        /// Gets or sets the content types.
        /// </summary>
        /// <value>
        /// The content types.
        /// </value>
        public IList<SPContentTypeId> ContentTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the managed properties.
        /// </summary>
        /// <value>
        /// The managed properties.
        /// </value>
        public IList<string> ManagedProperties { get; set; }

        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        public IList<SiteColumnField> Segments { get; set; }

        /// <summary>
        /// Gets or sets the default values.
        /// </summary>
        /// <value>
        /// The default values.
        /// </value>
        public IList<SiteColumnField> DefaultValues { get; set; }

        /// <summary>
        /// Gets or sets the field display settings.
        /// </summary>
        /// <value>
        /// The field display settings.
        /// </value>
        public IList<SiteColumnField> FieldDisplaySettings { get; set; }
    }
}
