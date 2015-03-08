using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Newtonsoft.Json;

namespace GSoft.Dynamite.ContentTypes
{
    /// <summary>
    /// Encapsulates Content Type metadata and structure information
    /// </summary>
    public class ContentTypeInfo : BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for ContentTypeInfo for serialization purposes
        /// </summary>
        public ContentTypeInfo()
        {
            this.Fields = new List<IFieldInfo>();
        }

        /// <summary>
        /// Initializes a new ContentTypeInfo
        /// </summary>
        /// <param name="contentTypeId">The content type identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public ContentTypeInfo(string contentTypeId, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : this(contentTypeId, displayNameResourceKey, descriptionResourceKey, groupResourceKey, string.Empty)
        { 
        }

        /// <summary>
        /// Initializes a new ContentTypeInfo
        /// </summary>
        /// <param name="contentTypeId">The content type identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        /// <param name="resourceFileName">Name of the resource file.</param>
        public ContentTypeInfo(string contentTypeId, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey, string resourceFileName)
            : this(new SPContentTypeId(contentTypeId), displayNameResourceKey, descriptionResourceKey, groupResourceKey, resourceFileName)
        {
        }

        /// <summary>
        /// Initializes a new ContentTypeInfo
        /// </summary>
        /// <param name="contentTypeId">The content type identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        public ContentTypeInfo(SPContentTypeId contentTypeId, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : this(contentTypeId, displayNameResourceKey, descriptionResourceKey, groupResourceKey, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new ContentTypeInfo
        /// </summary>
        /// <param name="contentTypeId">The content type identifier</param>
        /// <param name="displayNameResourceKey">Display name resource key</param>
        /// <param name="descriptionResourceKey">Description resource key</param>
        /// <param name="groupResourceKey">Content group resource key</param>
        /// <param name="resourceFileName">Name of the resource file.</param>
        public ContentTypeInfo(SPContentTypeId contentTypeId, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey, string resourceFileName)
            : base(displayNameResourceKey, descriptionResourceKey, groupResourceKey, resourceFileName)
        {
            this.ContentTypeId = contentTypeId;
            this.Fields = new List<IFieldInfo>();
        }

        /// <summary>
        /// The content type identifier
        /// </summary>
        [JsonIgnore]
        public SPContentTypeId ContentTypeId { get; private set; }

        /// <summary>
        /// String representation of the content type ID,
        /// convenient for serialization/deserialization.
        /// </summary>
        public string ContentTypeIdAsString 
        { 
            get
            {
                return this.ContentTypeId.ToString();
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.ContentTypeId = new SPContentTypeId(value);
                }
            }
        }

        /// <summary>
        /// Field description for all of the content type's fields (not including fields from parent content types)
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Allow overwrite of backing store to enable easier initialization of object.")]
        public ICollection<IFieldInfo> Fields { get; set; }
    }
}
