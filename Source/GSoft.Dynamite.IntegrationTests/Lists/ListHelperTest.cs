using System;
using Autofac;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Lists
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="ListHelper"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class ListHelperTest
    {
        #region "Ensure" should mean "Create if new or return existing"

        /// <summary>
        /// Validates that EnsureList creates a new list at the correct URL if it did not exist previously.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenNotAlreadyExists_ShouldCreateANewOne()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo("testUrl", "nameKey", "descriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;
                    var numberOfListsBefore = testRootWeb.Lists.Count;

                    SPList list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.AreEqual(listInfo.DescriptionResourceKey, list.DescriptionResource.Value);

                    // Fetch the list on the root web to make sure it was created and that it persists at the right location
                    var newlyCreatedList = testRootWeb.GetList("testUrl");

                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureList returns the existing list if one with that name already exists at that exact same URL.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenListAlreadyExistsAtThatURL_ShouldReturnExistingOne()
        {
            
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified, even though a list with the 
        /// same name already exists at a different URL.
        /// </summary>
        [TestMethod]        
        public void EnsureList_WhenListWithSameNameExistsButDifferentUrl_ShouldCreateNewOne()
        {
            
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified of a sub web one
        /// level under the root web, when no list with that name already exist there.
        /// </summary>
        [TestMethod]
        public void EnsureList_ListDoesntExistAndWantToCreateOnASubWebOneLevelUnderRoot_ShouldCreateAtCorrectUrl()
        {
            
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified (and at the specified web)
        /// even if a list with the same name already exists on a different web.
        /// </summary>
        public void EnsureList_AListWithSameNameExistsOnDifferentWeb_ShouldCreateListAtSpecifiedWebAndURL()
        {
            
        }

        #endregion
    }
}
