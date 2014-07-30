using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Security;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Setup
{   
    /// <summary>
    /// Adds pages to the Pages library
    /// </summary>
    public class PageCreator
    {
        private FolderRepository _folderRepository;

        /// <summary>
        /// PageCreator constructor
        /// </summary>
        /// <param name="folderRepository">Folder repository</param>
        public PageCreator(FolderRepository folderRepository)
        {
            this._folderRepository = folderRepository;
        }

        /// <summary>
        /// Creates a page in the Pages library
        /// </summary>
        /// <param name="currentWeb">The current web</param>
        /// <param name="folderId">The folder in which to add the item</param>
        /// <param name="contentTypeId">Id of Content Type to create</param>
        /// <param name="pageLayoutName">Name (filename) of Page Layout to apply</param>
        /// <param name="pageTitle">The human-readable title of the page</param>
        /// <param name="pageName">The url/name of the page relative to its parent folder</param>
        /// <returns>The newly created publishing page</returns>
        public PublishingPage Create(SPWeb currentWeb, int folderId, SPContentTypeId contentTypeId, string pageLayoutName, string pageTitle, string pageName)
        {
            PublishingPage newPage = null;

            var folder = this._folderRepository.GetFolderByIdForWeb(currentWeb, folderId);

            if (folder.Item.DoesUserHavePermissions(SPBasePermissions.AddListItems))
            {
                using (new Unsafe(currentWeb))
                {
                    SPContentType pubPageBaseContentType = currentWeb.AvailableContentTypes[ContentTypeId.ArticlePage];

                    var requestedContentType = currentWeb.AvailableContentTypes[contentTypeId];
                    if (null != requestedContentType && requestedContentType.Id.IsChildOf(pubPageBaseContentType.Id))
                    {
                        var publishingSite = new PublishingSite(currentWeb.Site);
                        PageLayoutCollection pageLayoutsForCT = publishingSite.GetPageLayouts(requestedContentType, false);

                        var requestedPageLayout = pageLayoutsForCT.Cast<PageLayout>().FirstOrDefault(layout => layout.Name == pageLayoutName);

                        if (requestedPageLayout != null)
                        {
                            PublishingWeb publishingWeb = PublishingWeb.GetPublishingWeb(currentWeb);

                            if (!pageName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                            {
                                pageName += ".aspx";
                            }

                            newPage = publishingWeb.GetPublishingPages().Add(folder.ServerRelativeUrl + "/" + pageName, requestedPageLayout);
                            newPage.ListItem[BuiltInFields.Title.InternalName] = pageTitle;
                            newPage.ListItem[BuiltInFields.ContentType.InternalName] = requestedContentType.Name;
                            newPage.ListItem[BuiltInFields.ContentTypeId.InternalName] = requestedContentType.Id;
                            newPage.ListItem.Update();
                        }
                    }
                }
            }

            return newPage;
        }

        /// <summary>
        /// Get name of default content type of folder based on its metadata
        /// or parent folders' metadata.
        /// </summary>
        /// <param name="folder">The folder in question</param>
        /// <returns>The content type id as string</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Dependency-injected classes should expose non-static members only for consistency.")]
        public string GetDefaultContentTypeId(SPFolder folder)
        {
            string contentTypeId = string.Empty;

            if (folder.Item != null)
            {
                MetadataDefaults metadata = new MetadataDefaults(folder.Item.ParentList);

                while (string.IsNullOrEmpty(contentTypeId) && folder != null)
                {
                    contentTypeId = metadata.GetFieldDefault(folder, BuiltInFields.ContentTypeId.InternalName);
                    folder = folder.ParentFolder;
                }
            }

            return contentTypeId;
        }

        /// <summary>
        /// Get the page layout
        /// </summary>
        /// <param name="publishingSite">the current publishing site</param>
        /// <param name="pageLayoutName">the page layout name</param>
        /// <param name="excludeObsolete">exclude obsolete page layout</param>
        /// <returns>the page layout</returns>
        public PageLayout GetPageLayout(PublishingSite publishingSite, string pageLayoutName, bool excludeObsolete)
        {
            return publishingSite.GetPageLayouts(excludeObsolete).FirstOrDefault(pageLayout => pageLayout.Name == pageLayoutName);
        }
    }
}
