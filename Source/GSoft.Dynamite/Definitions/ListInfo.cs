using System.Collections;
using System.Collections.Generic;
using GSoft.Dynamite.Lists;
using Microsoft.Office.Server.ApplicationRegistry.MetadataModel;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition for a list
    /// </summary>
    public class ListInfo : BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public ListInfo()
        {
            // Default value
            this.WriteSecurity = WriteSecurityOptions.AllUser;
            this.Overwrite = false;

            this.ContentTypes = new List<ContentTypeInfo>();
            this.DefaultViewFields = new List<IFieldInfo>();
            this.FieldDefinitions = new List<IFieldInfo>();
        }
        
        /// <summary>
        /// Initializes a new ListInfo
        /// </summary>
        /// <param name="webRelativeUrl">The web-relative URL of the list</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Description resource key</param>
        public ListInfo(string webRelativeUrl, string displayNameResourceKey, string descriptionResourceKey)
            : base(displayNameResourceKey, descriptionResourceKey, string.Empty)
        {
            // Default value
            this.WriteSecurity = WriteSecurityOptions.AllUser;
            this.Overwrite = false;

            this.ContentTypes = new List<ContentTypeInfo>();
            this.DefaultViewFields = new List<IFieldInfo>();
            this.FieldDefinitions = new List<IFieldInfo>();
        }

        /// <summary>
        /// Gets or sets the root folder URL.
        /// </summary>
        /// <value>
        /// The root folder URL.
        /// </value>
        public string RootFolderUrl { get; set; }

        /// <summary>
        /// Gets or sets the list template identifier.
        /// </summary>
        /// <value>
        /// The list template identifier.
        /// </value>
        public SPListTemplateType ListTemplate { get; set; }

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
        /// Content types definitions. If content types are specified, content type management
        /// should be turned on in your list. If not content types are specified, the collection
        /// of FieldDefinitions should be used to add fields to your list.
        /// </summary>
        public ICollection<ContentTypeInfo> ContentTypes { get; set; }

        /// <summary>
        /// Add the list to quick launch
        /// </summary>
        public bool AddToQuickLaunch { get; set; }

        /// <summary>
        /// The default view fields for the list
        /// </summary>
        public ICollection<IFieldInfo> DefaultViewFields { get; set; }

        /// <summary>
        /// List field definitions. Use to override site column definitions that come from ContentTypeInfo.
        /// If no ContentTypes are specified, these definitions should be used to add columns directly on
        /// your custom list.
        /// </summary>
        public ICollection<IFieldInfo> FieldDefinitions { get; set; }
    }
}
