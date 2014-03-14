namespace GSoft.Dynamite.WebParts
{
    using Microsoft.SharePoint.Publishing;

    /// <summary>
    /// The DefaultPageWebParts interface.
    /// </summary>
    public interface IDefaultPageWebParts
    {
        /// <summary>
        /// Add the right web parts to the page
        ///  </summary>
        /// <param name="publishingPage">the page</param>
        void AddWebPartsToPage(PublishingPage publishingPage);
    }
}
