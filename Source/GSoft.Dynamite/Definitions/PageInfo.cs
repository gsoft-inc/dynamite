using System.Collections.Generic;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a publishing page
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Default PageInfo constructor for serialization purposes
        /// </summary>
        public PageInfo()
        {
            this.WebParts = new Dictionary<string, WebPartInfo>();
        }

        /// <summary>
        /// Name of the file for the page
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Title of the page
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The page layout of the page
        /// </summary>
        public PageLayoutInfo PageLayout { get; set; }

        /// <summary>
        /// The content type of the page
        /// </summary>
        public string ContentTypeId { get; set; }

        /// <summary>
        /// WebParts by zone 
        /// </summary>
        public IDictionary<string, WebPartInfo> WebParts { get; set; } 
    }
}
