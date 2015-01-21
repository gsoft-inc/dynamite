using System;
using System.Collections.Generic;
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
        /// Validates that EnsureList creates a new list at the correct URL (standard /Lists/ path),
        /// if it did not exist previously.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenNotAlreadyExists_ShouldCreateNewOneAtListsPath()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo("Lists/testUrl", "nameKey", "descriptionKey");

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
                    var newlyCreatedList = testRootWeb.GetList("Lists/testUrl");

                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the correct URL (NOT relative to /Lists/)
        /// if it did not exist previously.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenNotAlreadyExists_ShouldCreateANewOneNOTListsPath()
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
            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo("testUrl", "nameKey", "descriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;

                    // 1- Create the list
                    var numberOfListsBefore = testRootWeb.Lists.Count;
                    SPList list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    
                    var newlyCreatedList = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);

                    // 2- Ensure the list a second time, now that it's been created
                    SPList expectingListCreatedAtStep1 = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, expectingListCreatedAtStep1.TitleResource.Value);
                    Assert.AreEqual(listInfo.DescriptionResourceKey, expectingListCreatedAtStep1.DescriptionResource.Value);

                    var listCreatedAtStep1 = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(listCreatedAtStep1);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listCreatedAtStep1.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified, even though a list with the 
        /// same name already exists at a different URL.
        /// </summary>
        [TestMethod]        
        public void EnsureList_WhenListWithSameNameExistsButDifferentUrl_ShouldCreateNewOne()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                const string SameNameKey = "nameKey";
                const string SameDescriptionKey = "descriptionKey";
                var listInfo = new ListInfo("testUrl", SameNameKey, SameDescriptionKey);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;

                    // 1- Create (by EnsureList) a first list at "testUrl"
                    var numberOfListsBefore = testRootWeb.Lists.Count;
                    SPList list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);

                    var newlyCreatedList = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);

                    // 2- Now, attempt to create a list with the same name at a different URL ("/Lists/secondUrl")
                    const string SecondUrl = "Lists/secondUrl";
                    var secondListInfo = new ListInfo(SecondUrl, SameNameKey, SameDescriptionKey);
                    SPList secondList = listHelper.EnsureList(testRootWeb, secondListInfo);

                    Assert.AreEqual(numberOfListsBefore + 2, testRootWeb.Lists.Count);
                    Assert.IsNotNull(secondList);

                    var secondCreatedList = testRootWeb.GetList(SecondUrl);
                    Assert.IsNotNull(secondCreatedList);
                    Assert.AreEqual(secondListInfo.DisplayNameResourceKey, secondCreatedList.TitleResource.Value);

                    // Check to see if the first list is still there
                    var regettingFirstList = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(regettingFirstList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, regettingFirstList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified of a sub web one
        /// level under the root web, when no list with that name already exist there.
        /// </summary>
        [TestMethod]
        public void EnsureList_ListDoesntExistAndWantToCreateOnASubWebOneLevelUnderRoot_ShouldCreateAtCorrectUrl()
        {
            // TODO: Waiting to see the changes made to ListHelper implementation
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified (and at the specified web)
        /// even if a list with the same name already exists on a different web.
        /// </summary>
        [TestMethod]
        public void EnsureList_AListWithSameNameExistsOnDifferentWeb_ShouldCreateListAtSpecifiedWebAndURL()
        {
            // TODO: Waiting to see the changes made to ListHelper implementation
        }

        #endregion
    }
}
