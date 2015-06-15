using System;
using System.Linq;
using Autofac;
using GSoft.Dynamite.ReusableContent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.ReusableContent
{
    /// <summary>
    /// Reusable Content Helper Integration tests
    /// </summary>
    [TestClass]
    public class ReusableContentHelperTest
    {
        #region GetByTitle

        /// <summary>
        /// Making sure EnsureList works fine when site collection is on a managed path.
        /// </summary>
        [TestMethod]
        public void GetByTitle_WhenGetForOOTBReusableContent_ShouldReturnReusableContent()
        {
            // Arrange
            using (var testScope = SiteTestScope.PublishingSite())
            {
                var site = testScope.SiteCollection;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var reusableContentHelper = injectionScope.Resolve<IReusableContentHelper>();

                    // Act
                    var copyright = reusableContentHelper.GetByTitle(site, "Copyright");

                    // Assert
                    Assert.IsNotNull(copyright);
                    Assert.AreEqual("None", copyright.Category);
                    Assert.IsTrue(copyright.IsAutomaticUpdate);
                    Assert.IsTrue(copyright.IsShowInRibbon);
                    Assert.IsTrue(copyright.Content.Contains("©"));
                }
            }
        }

        /// <summary>
        /// Making sure EnsureList works fine when site collection is on a managed path.
        /// </summary>
        [TestMethod]
        public void GetByTitle_WhenGetForNonExistingTitle_ShouldReturnNull()
        {
            // Arrange
            using (var testScope = SiteTestScope.PublishingSite())
            {
                var site = testScope.SiteCollection;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var reusableContentHelper = injectionScope.Resolve<IReusableContentHelper>();

                    // Act
                    var copyright = reusableContentHelper.GetByTitle(site, "IDoNotExist");

                    // Assert
                    Assert.IsNull(copyright);
                }
            }
        }

        #endregion GetByTitle

        #region GetAllReusableContentTitles

        /// <summary>
        /// Making sure EnsureList works fine when site collection is on a managed path.
        /// </summary>
        [TestMethod]
        public void GetAllReusableContentTitles_WhenGetAllOOTBTitles_ShouldReturn3Titles()
        {
            // Arrange
            using (var testScope = SiteTestScope.PublishingSite())
            {
                var site = testScope.SiteCollection;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var reusableContentHelper = injectionScope.Resolve<IReusableContentHelper>();

                    // Act
                    var allOOTBTitles = reusableContentHelper.GetAllReusableContentTitles(site);

                    // Assert
                    Assert.AreEqual(3, allOOTBTitles.Count());
                    Assert.IsTrue(allOOTBTitles.Contains("Copyright"));
                }
            }
        }

        #endregion GetAllReusableContentTitles
    }
}