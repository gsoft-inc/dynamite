using System.Linq;
using Autofac;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Extensions;
using GSoft.Dynamite.Pages;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Pages
{
    /// <summary>
    /// Validates the behavior of the default-configured implementation 
    /// of <see cref="IPageHelper"/>, the mapper interface.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class PageHelperTest
    {
        /// <summary>
        /// Validates that using the IPageHelper to Update an existing page with a new 
        /// Page Layout Info, Then the page is updated to use the new page layout.
        /// </summary>
        [TestMethod]
        public void EnsurePage_WhenUpdatingAPage_GivenNewPageLayoutInfo_ThenExistingPageUsesTheNewPageLayout()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                var pagesLibrary = testScope.SiteCollection.RootWeb.GetPagesLibrary();
                var folder = pagesLibrary.RootFolder;

                // Prepare the two page layouts. These are default SharePoint page Layouts.
                var initialPageLayoutInfo = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");
                var finalPageLayoutInfo = new PageLayoutInfo("ArticleRight.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF3900242457EFB8B24247815D688C526CD44D");

                // Prepare the two page infos. This for one, better simulates running the same code twice, and second we can't change the page layout property directly.
                var pageFileName = "TestPage";
                var initialPageInfo = new PageInfo(pageFileName, initialPageLayoutInfo);
                var updatedPageInfo = new PageInfo(pageFileName, finalPageLayoutInfo);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var pageHelper = injectionScope.Resolve<IPageHelper>();

                    // Act

                    // Ensure the page with the initial page layout
                    PublishingPage initialPage = pageHelper.EnsurePage(pagesLibrary, folder, initialPageInfo);

                    // Re ensure the same page with the final pageLayoutInfo
                    PublishingPage updatedPage = pageHelper.EnsurePage(pagesLibrary, folder, updatedPageInfo);

                    // Assert

                    // Make sure the page layout has truely changed.
                    Assert.AreNotEqual(initialPage.Layout.ServerRelativeUrl, updatedPage.Layout.ServerRelativeUrl, "Page layout url should not be the same on the inital page as the updated page.");

                    // Make sure we are truely talking about the same page.
                    Assert.AreEqual(initialPage.Url, updatedPage.Url, "The initial page and updated page should have the same url.");
                }
            }
        }

        /// <summary>
        /// When setting the associated content type to an existing page layout, then the page layout is published with a major version once updated.
        /// </summary>
        [TestMethod]
        public void EnsurePageLayout_WhenSettingTheAssociatedContentType_ThenThePageLayoutIsPublishedWithAMajorVersion()
        {
            using (var testScope = SiteTestScope.PublishingSite())
            {
                // Arrange

                // This page layout normally has article, we will set welcome page to it.
                var pageLayoutInfoWithUpdatedCT = new PageLayoutInfo("ArticleLeft.aspx", "0x010100C568DB52D9D0A14D9B2FDCC96666E9F2007948130EC3DB064584E219954237AF390064DEA0F50FC8C147B0B6EA0636C4A7D4");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(testScope.SiteCollection))
                {
                    var pageHelper = injectionScope.Resolve<IPageHelper>();

                    // Act
                    var actualPageLayout = pageHelper.EnsurePageLayout(testScope.SiteCollection, pageLayoutInfoWithUpdatedCT);

                    // Assert
                    Assert.IsTrue(actualPageLayout != null);

                    var hasCurrentDraft = actualPageLayout.ListItem.Versions.Cast<SPListItemVersion>().Any(v => v.IsCurrentVersion && v.Level == SPFileLevel.Draft);
                    Assert.IsTrue(!hasCurrentDraft, "The page layout should not have a draft version marked as current.");
                }
            }
        }
    }
}
