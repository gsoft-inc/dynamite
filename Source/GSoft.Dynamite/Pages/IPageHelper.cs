using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Helper for SharePoint publishing pages
    /// </summary>
    public interface IPageHelper
    {
        /// <summary>
        /// Ensure a collection of pages in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="pages">The page information</param>
        /// <returns>A collection of publishing pages</returns>
        IEnumerable<PublishingPage> EnsurePage(SPList library, SPFolder folder, ICollection<PageInfo> pages);

        /// <summary>
        /// Ensure a publishing page in a folder
        /// </summary>
        /// <param name="library">The library</param>
        /// <param name="folder">The folder</param>
        /// <param name="page">The page information</param>
        /// <returns>The publishing page object</returns>
        PublishingPage EnsurePage(SPList library, SPFolder folder, PageInfo page);

        /// <summary>
        /// Get the page layout
        /// </summary>
        /// <param name="publishingSite">the current publishing site</param>
        /// <param name="pageLayoutName">the page layout name</param>
        /// <param name="excludeObsolete">exclude obsolete page layout</param>
        /// <returns>the page layout</returns>
        PageLayout GetPageLayout(PublishingSite publishingSite, string pageLayoutName, bool excludeObsolete);

        /// <summary>
        /// Configures a page layout
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="pageLayoutInfo">The page layout info</param>
        /// <returns>The page layout</returns>
        PageLayout EnsurePageLayout(SPSite site, PageLayoutInfo pageLayoutInfo);

        /// <summary>
        /// Method that take a list of PageLayoutInfo and Set them as the Available Page Layout. 
        /// </summary>
        /// <param name="site">The Site Collection to Set the available Page Layout</param>
        /// <param name="pageLayoutInfos">The List of Page Layout Info to set as default</param>
        void SetAvailablePageLayouts(SPSite site, IList<PageLayoutInfo> pageLayoutInfos);
    }
}
