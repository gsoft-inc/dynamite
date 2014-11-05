using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using GSoft.Dynamite.Events;
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

        /// <summary>
        /// Initializes a new <see cref="PageHelper"/> instance
        /// </summary>
        /// <param name="webPartHelper">Web Part helper</param>
        public PageHelper(IWebPartHelper webPartHelper)
        {
            this.webPartHelper = webPartHelper;
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
            var publishingWeb = PublishingWeb.GetPublishingWeb(library.ParentWeb);
            var publishingPages = publishingWeb.GetPublishingPages();

            PageLayout pageLayout;

            // Get the correct Page Layout according to its name (<xxx>.aspx)
            var pageLayoutInfo = this.GetPageLayout(publishingSite, page.PageLayout.Name, true);

            // If a page layout was specified and its from the correct web.
            if (pageLayoutInfo != null && pageLayoutInfo.ListItem.ParentList.ParentWeb.ID == publishingSite.RootWeb.ID)
            {
                // Associate the page layout specified in the page.
                pageLayout = pageLayoutInfo;
            }
            else
            {
                // Get the first page layout with the specified content type id.
                var pageContentType = publishingSite.ContentTypes[page.ContentTypeId];
                var pageLayouts = publishingSite.GetPageLayouts(pageContentType, true);
                pageLayout = pageLayouts[0]; // default to first associated page layout
            }

            var pageServerRelativeUrl = folder.ServerRelativeUrl + "/" + page.FileName + ".aspx";
            var publishingPage = publishingWeb.GetPublishingPage(pageServerRelativeUrl);

            if (publishingPage == null)
            {
                // Only create the page if it doesn't exist yet and allow event firing on ItemAdded
                publishingPage = publishingPages.Add(pageServerRelativeUrl, pageLayout);
            }

            // Set the title
            if (!string.IsNullOrEmpty(page.Title))
            {
                publishingPage.Title = page.Title;
                publishingPage.Update();
            }

            // Insert WebParts
            foreach (WebPartInfo webPartSetting in page.WebParts)
            {
                this.webPartHelper.EnsureWebPartToZone(publishingPage.ListItem, webPartSetting.WebPart, webPartSetting.ZoneName, 1);
            }

            // Publish
            if (page.IsPublished)
            {
                publishingPage.ListItem.File.CheckIn("Dynamite Ensure Creation");
                publishingPage.ListItem.File.Publish("Dynamite Ensure Creation");
            }
            
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

            if (!string.IsNullOrEmpty(pageLayoutInfo.AssociatedContentTypeId))
            {
                var contentTypeId =
                site.RootWeb.ContentTypes.BestMatch(new SPContentTypeId(pageLayoutInfo.AssociatedContentTypeId));

                var ct = site.RootWeb.ContentTypes[contentTypeId];

                // Update the publishing associated content type
                pageLayout.AssociatedContentType = ct;
                pageLayout.Update();
            }

            return pageLayout;
        }
    }
}
