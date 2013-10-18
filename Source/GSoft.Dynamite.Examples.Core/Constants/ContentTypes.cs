using Microsoft.SharePoint;

namespace GSoft.Dynamite.Examples.Core.Constants
{
    /// <summary>
    /// Content Type constants.
    /// </summary>
    public static class ContentTypes
    {
        /// <summary>
        /// The wall posts content type id
        /// </summary>
        public static readonly SPContentTypeId WallPostsContentTypeId = new SPContentTypeId("0x0100a58d9c760cf4455d98a992dc0c41f0c8");

        /// <summary>
        /// The wall replies content type id
        /// </summary>
        public static readonly SPContentTypeId WallReplyContentTypeId = new SPContentTypeId("0x0100a5bdacbfc9aa4742ad9c3b5a469f59a8");
    }
}
