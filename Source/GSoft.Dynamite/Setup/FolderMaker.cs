using System;
using System.Collections.Generic;
using System.Globalization;
using GSoft.Dynamite.Logging;

using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Setup
{
    using GSoft.Dynamite.WebParts;

    /// <summary>
    /// Helps in constructing a translatable folder tree of <see cref="FolderInfo"/>
    /// </summary>
    public class FolderMaker : IFolderMaker
    {
        private readonly ILogger logger;
        private readonly IDefaultPageWebPartIndex defaultPageWebPartIndex;

        /// <summary>
        /// Constructor for <see cref="FolderMaker"/>
        /// </summary>
        /// <param name="logger">
        /// Logging utility
        /// </param>
        /// <param name="defaultPageWebPartIndex">
        /// The default Page Web Part Index.
        /// </param>
        public FolderMaker(ILogger logger, IDefaultPageWebPartIndex defaultPageWebPartIndex)
        {
            this.logger = logger;
            this.defaultPageWebPartIndex = defaultPageWebPartIndex;
        }

        /// <summary>
        /// Builds a translatable folder hierarchy
        /// </summary>
        /// <param name="library">The document library</param>
        /// <param name="rootFolderInfo">The metadata for initializing the folder at the root of the library</param>
        public void Make(SPList library, IFolderInfo rootFolderInfo)
        {
            this.RecursiveMake(library, null, rootFolderInfo);
        }

        private static void ApplyValuesAndDefaults(SPList library, SPFolder folder, SPListItem folderItem, IFolderInfo folderInfo)
        {
            MetadataDefaults metadataDefaults = null;

            if (folderInfo.Values != null)
            {
                foreach (var value in folderInfo.Values)
                {
                    if (value == null)
                    {
                        continue;
                    }

                    var taxonomyInfo = value as TaxonomyInfo;
                    var taxonomyMultiInfo = value as TaxonomyMultiInfo;
                    if (taxonomyInfo != null)
                    {
                        taxonomyInfo.ApplyOnItem(folderItem, library);
                    }
                    else if (taxonomyMultiInfo != null)
                    {
                        taxonomyMultiInfo.ApplyOnItem(folderItem, library);
                    }
                    else
                    {
                        value.ApplyOnItem(folderItem);
                    }
                }
            }

            if (folderInfo.Defaults != null)
            {
                metadataDefaults = new MetadataDefaults(library);

                foreach (var metaDefault in folderInfo.Defaults)
                {
                    var taxonomyDefault = metaDefault as TaxonomyInfo;
                    var taxonomyMultiInfo = metaDefault as TaxonomyMultiInfo;
                    if (taxonomyDefault != null)
                    {
                        taxonomyDefault.ApplyFieldOnMetadata(metadataDefaults, folder, library);
                    }
                    else if (taxonomyMultiInfo != null)
                    {
                        taxonomyMultiInfo.ApplyFieldOnMetadata(metadataDefaults, folder, library);
                    }
                    else
                    {
                        metaDefault.ApplyFieldOnMetadata(metadataDefaults, folder);
                    }
                }
            }

            if (folderInfo.UniqueContentTypeOrder.Count > 0)
            {
                var listContentTypes = new List<SPContentType>();
                foreach (var contentTypeId in folderInfo.UniqueContentTypeOrder)
                {
                    // Get the content type id for this particular list.
                    var listContentTypeId = library.ContentTypes.BestMatch(contentTypeId);

                    // Make sure it is the direct child of the one we specified.
                    if (listContentTypeId.Parent == contentTypeId)
                    {
                        // Add it to the list of content types.
                        listContentTypes.Add(library.ContentTypes[listContentTypeId]);
                    }
                }

                // Set the content types to the folder.
                if (listContentTypes.Count > 0)
                {
                    folder.UniqueContentTypeOrder = listContentTypes;
                }
            }

            if (metadataDefaults != null)
            {
                metadataDefaults.Update();
            }
        }

        private static void ApplyPageValues(SPListItem item, IPageInfo page)
        {
            if (page.Values != null)
            {
                foreach (FieldValueInfo value in page.Values)
                {
                    value.ApplyOnItem(item);
                }
            }

            item.Update();
        }

        private static void EnsurePageCheckInAndPublish(PublishingPage page)
        {
            if (page.ListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
            {
                // Only check in if already checked out
                page.CheckIn(string.Empty);
            }

            if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Draft)
            {
                // Create a major version (just like "submit for approval")
                page.ListItem.File.Publish(string.Empty);

                // Status should now be Pending. Approve to make the major version visible to the public.
                page.ListItem.File.Approve(string.Empty);
            }
            else if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Pending)
            {
                // Technically, major version already exists, we just need to approve in order for the major version to be published
                page.ListItem.File.Approve(string.Empty);
            }
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

        private void RecursiveMake(SPList library, SPFolder parent, IFolderInfo folderInfo)
        {
            // Add the folder (if it doesn't already exist)
            SPFolder folder = null;

            if (parent == null)
            {
                // We are on at the root folder of the library (i.e. no parent folder).
                // Initialize defaults and pages in here, then move on to subfolders (instead of trying to create the folder)
                folder = library.RootFolder;

                // Metadata default in root folder (no values to add on item - anyway root folder doesn't have an associated Item)
                ApplyValuesAndDefaults(library, folder, null, folderInfo);

                // Page instances in root folder
                this.AddFolderPages(library, folder, folderInfo);
            }
            else if (!string.IsNullOrEmpty(folderInfo.Name))
            {
                try
                {
                    folder = parent.SubFolders[folderInfo.Name];
                    this.logger.Info("Skipping folder creation for " + folderInfo.Name + " because it already exists (will still apply values and default metadata)");
                }
                catch (ArgumentException)
                {
                    this.logger.Info("Creating folder " + folderInfo.Name);
                }

                if (folder == null)
                {
                    folder = parent.SubFolders.Add(folderInfo.Name);
                }

                // Re-apply the values and default, even if the folder was already created
                if (folder != null) 
                {
                    SPListItem folderItem = folder.Item;
                    ApplyValuesAndDefaults(library, folder, folderItem, folderInfo);
                    folderItem.Update();
                    folder.Update();

                    EnsureFolderPublish(folder);
                }

                // Add pages to each folder
                this.AddFolderPages(library, folder, folderInfo);
            }

            foreach (var childFolder in folderInfo.Subfolders ?? new List<IFolderInfo>())
            {
                this.RecursiveMake(library, folder, childFolder);
            }
        }

        private void AddFolderPages(SPList library, SPFolder folder, IFolderInfo folderInfo)
        {
            if (folderInfo.Pages != null)
            {
                var publishingSite = new PublishingSite(library.ParentWeb.Site);
                var publishingWeb = PublishingWeb.GetPublishingWeb(library.ParentWeb);
                var publishingPages = publishingWeb.GetPublishingPages();

                foreach (var page in folderInfo.Pages)
                {
                    PageLayout pageLayout;

                    // If a page layout was specified and its from the correct web.
                    if (page.PageLayout != null && page.PageLayout.ListItem.ParentList.ParentWeb.ID == publishingSite.RootWeb.ID)
                    {
                        // Associate the page layout specified in the page.
                        pageLayout = page.PageLayout;
                    }
                    else
                    {
                        // Get the first page layout with the specified content type id.
                        var pageContentType = publishingSite.ContentTypes[page.ContentTypeId];
                        var pageLayouts = publishingSite.GetPageLayouts(pageContentType, true);
                        pageLayout = pageLayouts[0]; // default to first associated page layout
                    }

                    var pageServerRelativeUrl = folder.ServerRelativeUrl + "/" + page.Name + ".aspx";
                    var existingPage = publishingWeb.GetPublishingPage(pageServerRelativeUrl);

                    if (existingPage == null)
                    {
                        // Only create the page if it doesn't exist yet
                        var publishingPage = publishingPages.Add(pageServerRelativeUrl, pageLayout);

                        this.EnsurePageCheckOut(publishingPage);
                        
                        var item = publishingPage.ListItem;

                        ApplyPageValues(item, page);

                        publishingPage.Update();

                        // Add webparts to the page
                        this.EnsureWebpartsOnPage(publishingPage);

                        if (page.IsWelcomePage)
                        {
                            folder.WelcomePage = item.Name;
                            folder.Update();
                            EnsureFolderPublish(folder);

                            if (folder.UniqueId == library.RootFolder.UniqueId)
                            {
                                // We are setting the Pages library's root folder's welcome page, so let's assume this means we also need to set it as the website's welcome page as well
                                var webRootFolder = library.ParentWeb.RootFolder;
                                webRootFolder.WelcomePage = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", publishingPages.PubWeb.PagesListName, publishingPage.Name);
                                webRootFolder.Update();

                                EnsureFolderPublish(folder);
                            }
                        }

                        EnsurePageCheckInAndPublish(publishingPage);
                    }
                    else
                    {
                        this.logger.Info("Folder maker is skipping the creation of page '{0}' because it already exists.", existingPage.Url);

                        if (this.defaultPageWebPartIndex.GetDefaultWebPartsForPageUrl(existingPage.Url) != null)
                        {
                            // If there are some registered on the index, add the 
                            // webparts to the page (make sure to checkout/checkin).
                            this.logger.Info("Ensuring the existance of the webparts on page '{0}'.", existingPage.Url);

                            this.EnsurePageCheckOut(existingPage);
                            this.EnsureWebpartsOnPage(existingPage);
                            EnsurePageCheckInAndPublish(existingPage);
                        }
                    }
                }
            }
        }

        private void EnsureWebpartsOnPage(PublishingPage publishingPage)
        {
            var pageWebPart = this.defaultPageWebPartIndex.GetDefaultWebPartsForPageUrl(publishingPage.Url);
            if (pageWebPart != null)
            {
                pageWebPart.AddWebPartsToPage(publishingPage);
            }
        }

        private void EnsurePageCheckOut(PublishingPage page)
        {
            if (page.ListItem.ParentList.ForceCheckout)
            {
                // Only check out if we are forced to do so
                if (page.ListItem.File.CheckOutType == SPFile.SPCheckOutType.None)
                {
                    // Only check out if not already checked out
                    page.CheckOut();
                }
                else
                {
                    this.logger.Warn("Page " + page.Uri.AbsoluteUri + " is already checked out - skipping FolderMaker checkout.");
                }
            }
        }
    }
}
