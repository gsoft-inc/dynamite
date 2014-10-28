namespace GSoft.Dynamite.Setup
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;

    public interface IPageCreator
    {
        /// <summary>
        /// Create publishing page in Pages Library
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="folderId">the current folder id</param>
        /// <param name="contentTypeId">the current content type id</param>
        /// <param name="pageLayoutName">the page layout name</param>
        /// <param name="pageTitle">the page title</param>
        /// <param name="pageName">the page name</param>
        /// <returns>the created publishing page</returns>
        PublishingPage Create(SPWeb web, int folderId, SPContentTypeId contentTypeId, string pageLayoutName, string pageTitle, string pageName);

        /// <summary>
        /// Creates a page in the Pages library
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="pageInfo">the pageInfo of the page</param>
        /// <returns>The newly created publishing page</returns>
        PublishingPage Create(SPWeb web, PageInfo pageInfo);

        /// <summary>
        /// Creates a page in the Pages library
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="folderId">The folder in which to add the item</param>
        /// <param name="pageInfo">The pageInfo of the page</param>
        /// <returns>The newly created publishing page</returns>
        PublishingPage Create(SPWeb web, int folderId, PageInfo pageInfo);

        /// <summary>
        /// Get the page layout
        /// </summary>
        /// <param name="publishingSite">the current publishing site</param>
        /// <param name="pageLayoutName">the page layout name</param>
        /// <param name="excludeObsolete">exclude obsolete page layout</param>
        /// <returns>the page layout</returns>
        PageLayout GetPageLayout(PublishingSite publishingSite, string pageLayoutName, bool excludeObsolete);

        /// <summary>
        /// Get name of default content type of folder based on its metadata
        /// or parent folders' metadata.
        /// </summary>
        /// <param name="folder">The folder in question</param>
        /// <returns>The content type id as string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        string GetDefaultContentTypeId(SPFolder folder);
    }
}