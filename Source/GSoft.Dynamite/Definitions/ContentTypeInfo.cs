using System.Collections.Generic;
using System.Globalization;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.Utils;

using Microsoft.SharePoint;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Encapsulates Content Type metadata and structure information
    /// </summary>
    public class ContentTypeInfo : BaseTypeInfo
    {
        /// <summary>
        /// Default constructor for ContentTypeInfo
        /// </summary>
        public ContentTypeInfo()
        {          
        }

        /// <summary>
        /// The content type identifier
        /// </summary>
        public string ContentTypeId { get; set; }

        /// <summary>
        /// Field description for all of the content type's fields (not including fields from parent content types)
        /// </summary>
        public ICollection<FieldInfo> Fields { get; set; }
    }
}
