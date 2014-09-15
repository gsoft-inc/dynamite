using System.Collections.Generic;
using System.Globalization;

using GSoft.Dynamite.Utils;

using Microsoft.SharePoint;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Encapsulates Content Type metadata and structure information
    /// </summary>
    public class ContentTypeInfo
    {
        /// <summary>
        /// Typical resource string format
        /// </summary>
        private const string DollarFormat = "$Resources:{0},{1};";

        private string resourceFileName;

        /// <summary>
        /// Default constructor for ContentTypeInfo
        /// </summary>
        public ContentTypeInfo()
        { 
        }

        /// <summary>
        /// Creates a content type metadata encapsulation
        /// </summary>
        /// <param name="contentTypeId">The content type ID</param>
        /// <param name="fields">A list of field information to document the structure of the content type</param>
        /// <param name="titleResourceKey">The resource key to use for the title</param>
        /// <param name="descriptionResourceKey">The resource key to use for the description</param>
        /// <param name="contentGroupResourceKey">The resource key to use for the content group</param>
        /// <param name="resourceFileName">The resource file where all the resources can be found</param>
        public ContentTypeInfo(SPContentTypeId contentTypeId, IList<FieldInfo> fields, string titleResourceKey, string descriptionResourceKey, string contentGroupResourceKey, string resourceFileName)
        {
            this.ContentTypeId = contentTypeId;
            this.TitleResourceKey = titleResourceKey;
            this.DescriptionResourceKey = descriptionResourceKey;
            this.ContentGroupResourceKey = contentGroupResourceKey;
            this.Fields = fields;
            this.resourceFileName = resourceFileName;
        }

        /// <summary>
        /// The content type ID
        /// </summary>
        public SPContentTypeId ContentTypeId { get; set; }

        /// <summary>
        /// Field description for all of the content type's fields (not including fields from parent content types)
        /// </summary>
        public IList<FieldInfo> Fields { get; set; }

        /// <summary>
        /// Resource key for the content type title
        /// </summary>
        public string TitleResourceKey { get; set; }

        /// <summary>
        /// Dollar-formatted resource string inferred from the TitleResourceKey
        /// </summary>
        public string TitleResourceString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, DollarFormat, this.resourceFileName, this.TitleResourceKey);
            }
        }

        /// <summary>
        /// Resource key for the content type description
        /// </summary>
        public string DescriptionResourceKey { get; set; }

        /// <summary>
        /// Dollar-formatted resource string inferred from the DescriptionResourceKey
        /// </summary>
        public string DescriptionResourceString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, DollarFormat, this.resourceFileName, this.DescriptionResourceKey);
            }
        }

        /// <summary>
        /// Resource key for the content type content group
        /// </summary>
        public string ContentGroupResourceKey { get; set; }

        /// <summary>
        /// Dollar-formatted resource string inferred from the ContentGroupResourceKey
        /// </summary>
        public string ContentGroupResourceString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, DollarFormat, this.resourceFileName, this.ContentGroupResourceKey);
            }
        }
    }
}
