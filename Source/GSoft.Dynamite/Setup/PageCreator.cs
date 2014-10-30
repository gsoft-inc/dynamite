using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Folders;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Repositories;
using GSoft.Dynamite.Security;
using Microsoft.Office.DocumentManagement;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.Web.Hosting.Administration;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Adds pages to the Pages library
    /// </summary>
    public class PageCreator : IPageCreator
    {
        private readonly IFolderRepository folderRepository;
        private readonly ILogger logger;

        /// <summary>
        /// PageCreator constructor
        /// </summary>
        /// <param name="folderRepository">Folder repository</param>
        /// <param name="logger">the current logger</param>
        public PageCreator(IFolderRepository folderRepository, ILogger logger)
        {
            this.folderRepository = folderRepository;
            this.logger = logger;
        }

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
        public PublishingPage Create(SPWeb web, int folderId, SPContentTypeId contentTypeId, string pageLayoutName, string pageTitle, string pageName)
        {
            var publishingSite = new PublishingSite(web.Site);
            var pageLayout = this.GetPageLayout(publishingSite, pageLayoutName, false);
            var page = new PageInfo()
            {
                Name = pageName,
                ContentTypeId = contentTypeId,
                PageLayout = pageLayout,
                Values = new List<IFieldValueInfo>()
                {
                    new FieldValueInfo()
                    {
                        FieldName = BuiltInFields.Title.InternalName,
                        Value = pageTitle
                    }
                }
            };

            return this.Create(web, folderId, page);
        }

        /// <summary>
        /// Creates a page in the Pages library
        /// </summary>
        /// <param name="web">the current web</param>
        /// <param name="pageInfo">the pageInfo of the page</param>
        /// <returns>The newly created publishing page</returns>
        public PublishingPage Create(SPWeb web, PageInfo pageInfo)
        {
            return this.Create(web, int.MinValue, pageInfo);
        }

        /// <summary>
        /// Creates a page in the Pages library
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="folderId">The folder in which to add the item</param>
        /// <param name="pageInfo">The pageInfo of the page</param>
        /// <returns>The newly created publishing page</returns>
        public PublishingPage Create(SPWeb web, int folderId, PageInfo pageInfo)
        {
            PublishingPage newPage = null;
            bool userHavePermissions = false;

            // get the root folder if no folder is specified
            var folder = folderId == int.MinValue ? web.GetPagesLibrary().RootFolder : this.folderRepository.GetFolderByIdForWeb(web, folderId);

            // if spfolder is root folder, check permissions at library level
            if (folder.Item == null)
            {
                userHavePermissions = folder.DocumentLibrary.DoesUserHavePermissions(SPBasePermissions.AddListItems);
            }
            else
            {
                userHavePermissions = folder.Item.DoesUserHavePermissions(SPBasePermissions.AddListItems);
            }

            if (userHavePermissions)
            {
                using (new Unsafe(web))
                {
                    var requestedContentType = web.AvailableContentTypes[pageInfo.ContentTypeId];

                    if (requestedContentType != null)
                    {
                        if (pageInfo.PageLayout != null)
                        {
                            var publishingWeb = PublishingWeb.GetPublishingWeb(web);

                            if (!pageInfo.Name.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                            {
                                pageInfo.Name += ".aspx";
                            }

                            newPage = publishingWeb.GetPublishingPages().Add(folder.ServerRelativeUrl + "/" + pageInfo.Name, pageInfo.PageLayout);
                            newPage.ListItem[BuiltInFields.ContentType.InternalName] = requestedContentType.Name;
                            newPage.ListItem[BuiltInFields.ContentTypeId.InternalName] = requestedContentType.Id;

                            if (pageInfo.Values != null)
                            {
                                foreach (var field in pageInfo.Values)
                                {
                                    newPage.ListItem[field.FieldName] = field.Value;
                                }
                            }

                            if (pageInfo.IsWelcomePage)
                            {
                                folder.WelcomePage = newPage.ListItem.Name;
                                folder.Update();
                                EnsureFolderPublish(folder);

                                if (folder.UniqueId == newPage.ListItem.ParentList.RootFolder.UniqueId)
                                {
                                    // We are setting the Pages library's root folder's welcome page, so let's assume this means we also need to set it as the website's welcome page as well
                                    var webRootFolder = newPage.ListItem.ParentList.ParentWeb.RootFolder;
                                    webRootFolder.WelcomePage = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", publishingWeb.PagesListName, newPage.Name);
                                    webRootFolder.Update();

                                    EnsureFolderPublish(folder);
                                }
                            }

                            newPage.ListItem.Update();
                        }
                    }
                }
            }

            return newPage;
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
            return publishingSite.GetPageLayouts(excludeObsolete).Cast<PageLayout>().FirstOrDefault(pageLayout => pageLayout.Name == pageLayoutName);
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
                var metadata = new MetadataDefaults(folder.Item.ParentList);

                while (string.IsNullOrEmpty(contentTypeId) && folder != null)
                {
                    contentTypeId = metadata.GetFieldDefault(folder, BuiltInFields.ContentTypeId.InternalName);
                    folder = folder.ParentFolder;
                }
            }

            return contentTypeId;
        }

        private static void EnsureFolderPublish(SPFolder folder)
        {
            if (folder.Item != null
                && folder.Item.ModerationInformation != null
                && folder.Item.ParentList.EnableModeration
                && folder.Item.ModerationInformation.Status != SPModerationStatusType.Approved)
            {
                // Only approve a folder if it isn't approved
                folder.Item.ModerationInformation.Status = SPModerationStatusType.Approved;
                folder.Item.Update();
            }
        }
    }
}
