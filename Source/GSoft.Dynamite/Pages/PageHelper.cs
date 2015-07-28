using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using GSoft.Dynamite.Events;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes.Writers;
using GSoft.Dynamite.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Helper class for SharePoint publishing pages
    /// </summary>
    public class PageHelper : IPageHelper
    {
        private readonly IWebPartHelper webPartHelper;
        private readonly ILogger logger;
        private readonly IFieldValueWriter itemValueWriter;

        /// <summary>
        /// Initializes a new <see cref="PageHelper" /> instance
        /// </summary>
        /// <param name="webPartHelper">Web Part helper</param>
        /// <param name="logger">The logger.</param>
        /// <param name="itemValueWriter">The item value writer.</param>
        public PageHelper(IWebPartHelper webPartHelper, ILogger logger, IFieldValueWriter itemValueWriter)
        {
            this.webPartHelper = webPartHelper;
            this.logger = logger;
            this.itemValueWriter = itemValueWriter;
        }

        /// <summary>
        /// Ensure a collection of pages in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="pages">The page information</param>
        /// <returns>A collection of publishing pages</returns>
        public IEnumerable<PublishingPage> EnsurePage(SPList library, SPFolder folder, ICollection<PageInfo> pages)
        {
            return pages.Select(page => this.EnsurePage(library, folder, page)).ToList();
        }

        /// <summary>
        /// Ensure a publishing page in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="page">The page information</param>
        /// <returns>The publishing page object</returns>
        public PublishingPage EnsurePage(SPList library, SPFolder folder, PageInfo page)
        {
            var publishingSite = new PublishingSite(library.ParentWeb.Site);

            if (!PublishingWeb.IsPublishingWeb(library.ParentWeb))
            {
                throw new ArgumentException("Publishing pages cannot be provisionned outside of a Publishing web (choose the Publishing Site or Enterprise Wiki site definition).");
            }

            var publishingWeb = PublishingWeb.GetPublishingWeb(library.ParentWeb);
            var publishingPages = publishingWeb.GetPublishingPages();

            PageLayout pageLayout = null;

            // Get the correct Page Layout according to its name (<xxx>.aspx)
            var pageLayoutInfo = this.GetPageLayout(publishingSite, page.PageLayout.Name, true);

            // If a page layout was specified and its from the correct web.
            if (pageLayoutInfo != null && pageLayoutInfo.ListItem.ParentList.ParentWeb.ID == publishingSite.RootWeb.ID)
            {
                // Associate the page layout specified in the page.
                pageLayout = pageLayoutInfo;
            }

            var pageServerRelativeUrl = folder.ServerRelativeUrl + "/" + page.FileName + ".aspx";
            Uri baseUri = new Uri(library.ParentWeb.Url, UriKind.Absolute);
            var publishingPage = publishingPages.ToList().Find(
                x => Uri.Compare(x.Uri, new Uri(baseUri, pageServerRelativeUrl), UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0);

            if (publishingPage == null)
            {
                // Only create the page if it doesn't exist yet and allow event firing on ItemAdded
                publishingPage = publishingPages.Add(pageServerRelativeUrl, pageLayout);
            }
            else
            {
                this.EnsurePageCheckOut(publishingPage);

                // Update the Page layout.
                publishingPage.Layout = pageLayout;
                publishingPage.Update();
            }

            // Set the title
            if (!string.IsNullOrEmpty(page.Title))
            {
                publishingPage.Title = page.Title;
                publishingPage.Update();
            }

            // Set field Values
            this.itemValueWriter.WriteValuesToListItem(publishingPage.ListItem, page.FieldValues);
            publishingPage.ListItem.Update();

            // Insert WebParts
            foreach (WebPartInfo webPartSetting in page.WebParts)
            {
                this.webPartHelper.EnsureWebPart(publishingPage.ListItem, webPartSetting);
            }

            // Publish
            PageHelper.EnsurePageCheckInAndPublish(page, publishingPage);

            return publishingPage;
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
        /// Configures a page layout
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="pageLayoutInfo">The page layout info</param>
        /// <returns>The page layout</returns>
        public PageLayout EnsurePageLayout(SPSite site, PageLayoutInfo pageLayoutInfo)
        {
            var publishingSite = new PublishingSite(site);
            var pageLayout = this.GetPageLayout(publishingSite, pageLayoutInfo.Name, true);

            if (pageLayoutInfo.AssociatedContentTypeId != null)
            {
                var contentTypeId =
                site.RootWeb.ContentTypes.BestMatch(pageLayoutInfo.AssociatedContentTypeId);

                var ct = site.RootWeb.ContentTypes[contentTypeId];

                // Applies the preview picture of the page layout
                if (pageLayoutInfo.PreviewImageUrl != null)
                {
                    Uri previewImageUrl;

                    if (!pageLayoutInfo.PreviewImageUrl.IsAbsoluteUri)
                    {
                        previewImageUrl = new Uri(new Uri(site.Url), pageLayoutInfo.PreviewImageUrl);
                    }
                    else
                    {
                        previewImageUrl = pageLayoutInfo.PreviewImageUrl;
                    }

                    pageLayout.PreviewImageUrl = previewImageUrl.AbsoluteUri;
                }

                // Update the publishing associated content type
                pageLayout.AssociatedContentType = ct;
                pageLayout.Update();
            }

            return pageLayout;
        }

        /// <summary>
        /// Method that take a list of PageLayoutInfo and Set them as the Available Page Layout. 
        /// </summary>
        /// <param name="site">The Site Collection to Set the available Page Layout</param>
        /// <param name="pageLayoutInfos">The List of Page Layout Info to set as default</param>
        public void SetAvailablePageLayouts(SPSite site, IList<PageLayoutInfo> pageLayoutInfos)
        {
            var publishingSite = new PublishingSite(site);
            var availablePageLayouts = new List<PageLayout>();
            var allPageLayouts = publishingSite.GetPageLayouts(false).Cast<PageLayout>();

            // Build the Available Page Layout list
            foreach (var pageLayout in allPageLayouts)
            {
                if (pageLayoutInfos.Any(x => x.Name == pageLayout.Name))
                {
                    availablePageLayouts.Add(pageLayout);
                }
            }

            // Set The Available Page Layouts for each Webs of the Site
            foreach (SPWeb web in site.AllWebs)
            {
                var publishingWeb = PublishingWeb.GetPublishingWeb(web);

                publishingWeb.SetAvailablePageLayouts(availablePageLayouts.ToArray(), true);
                publishingWeb.Update();
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

        private static void EnsurePageCheckInAndPublish(PageInfo pageinfo, PublishingPage page)
        {
            string comment = "Dynamite Ensure Creation";

            if (page.ListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
            {
                // Only check in if already checked out
                page.CheckIn(comment);
            }

            // Are we publishing this page or not ?
            if (pageinfo.IsPublished)
            {
                if (page.ListItem.ParentList.EnableModeration)
                {
                    if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Draft)
                    {
                        // Create a major version (just like "submit for approval")
                        page.ListItem.File.Publish(comment);

                        // Status should now be Pending. Approve to make the major version visible to the public.
                        page.ListItem.File.Approve(comment);
                    }
                    else if (page.ListItem.ModerationInformation.Status == SPModerationStatusType.Pending)
                    {
                        // Technically, major version already exists, we just need to approve in order for the major version to be published
                        page.ListItem.File.Approve(comment);
                    }
                }
                else if (page.ListItem.File.MinorVersion != 0)
                {
                    // Create a major version, No approval required for this case.
                    page.ListItem.File.Publish(comment);
                }
            }
        }
    }
}