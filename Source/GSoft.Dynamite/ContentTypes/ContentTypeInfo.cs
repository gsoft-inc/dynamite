using System.Collections.Generic;
using System.Globalization;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;

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
        /// <param name="groupResourceKey">Description resource key</param>
        public ContentTypeInfo(string contentTypeId, string displayNameResourceKey, string descriptionResourceKey, string groupResourceKey)
            : base(displayNameResourceKey, descriptionResourceKey, groupResourceKey)
        {
            this.ContentTypeId = contentTypeId;
            this.Fields = new List<IFieldInfo>();
        }

        /// <summary>
        /// The content type identifier
        /// </summary>
        public string ContentTypeId { get; private set; }

        /// <summary>
        /// Field description for all of the content type's fields (not including fields from parent content types)
        /// </summary>
        public ICollection<IFieldInfo> Fields { get; set; }
    }
}
