using System;
using System.Collections.Generic;
using Autofac;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
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
            const string Url = "Lists/testUrl";

            using (var testScope = SiteTestScope.BlankSite())
            {
                
                var listInfo = new ListInfo(Url, "nameKey", "descriptionKey");

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
                    var newlyCreatedList = testRootWeb.GetList(Url);

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
        public void EnsureList_WhenListWithSameNameAlreadyExistsAtThatURL_ShouldReturnExistingOne()
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
        /// Validates that EnsureList doesn't allow, on the same web, to create a new list if
        /// one with the same display name already exists, even if the relative URL is different.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_WhenListWithSameNameExistsButDifferentUrl_ShouldThrowException()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                const string SameNameKey = "nameKey";
                const string SameDescriptionKey = "descriptionKey";
                const string Url = "testUrl";
                var listInfo = new ListInfo(Url, SameNameKey, SameDescriptionKey);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;

                    // 1- Create (by EnsureList) a first list at "testUrl"
                    var numberOfListsBefore = testRootWeb.Lists.Count;
                    SPList list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);

                    var newlyCreatedList = testRootWeb.GetList(Url);
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);

                    // 2- Now, attempt to create a list with the same name at a different URL ("/Lists/secondUrl")
                    const string SecondUrl = "Lists/" + Url;
                    var secondListInfo = new ListInfo(SecondUrl, SameNameKey, SameDescriptionKey);
                    SPList secondListShouldThrowException = listHelper.EnsureList(testRootWeb, secondListInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNull(secondListShouldThrowException);

                    var secondCreatedList = testRootWeb.GetList(SecondUrl);
                    Assert.IsNull(secondCreatedList);

                    // Check to see if the first list is still there
                    var regettingFirstList = testRootWeb.GetList(Url);
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
            const string Url = "some/random/path";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Creating the ListInfo and the sub-web
                var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
                SPWeb subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnSubWebBefore = subWeb.Lists.Count;

                    SPList list = listHelper.EnsureList(subWeb, listInfo);

                    Assert.AreEqual(numberOfListsOnSubWebBefore + 1, subWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);

                    var newlyCreatedList = subWeb.GetList(SPUtility.ConcatUrls(subWeb.ServerRelativeUrl, Url));
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureList creates a new list at the URL specified (and at the specified web)
        /// even if a list with the same name already exists on a different web.
        /// </summary>
        [TestMethod]
        public void EnsureList_AListWithSameNameExistsOnDifferentWeb_ShouldCreateListAtSpecifiedWebAndURL()
        {
            const string Url = "testUrl";
            const string NameKey = "NameKey";
            const string DescKey = "DescriptionKey";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Let's first create a list on the root web
                var listInfo = new ListInfo(Url, NameKey, DescKey);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    SPList listRootWeb = listHelper.EnsureList(rootWeb, listInfo);
                    
                    Assert.AreEqual(numberOfListsOnRootWebBefore + 1, rootWeb.Lists.Count);
                    Assert.IsNotNull(listRootWeb);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listRootWeb.TitleResource.Value);

                    // Now let's create a sub web under root, and try to ensure the "same" list there. It should create a new one.
                    var subWeb = rootWeb.Webs.Add("subweb");
                    var numberOfListsOnSubWebBefore = subWeb.Lists.Count;

                    SPList listSubWeb = listHelper.EnsureList(subWeb, listInfo);
                    
                    Assert.AreEqual(numberOfListsOnSubWebBefore + 1, subWeb.Lists.Count);
                    Assert.IsNotNull(listSubWeb);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listSubWeb.TitleResource.Value);

                    // Finally, try to get both lists to make sure everything is right
                    var firstList = rootWeb.GetList(Url);
                    Assert.IsNotNull(firstList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, firstList.TitleResource.Value);

                    var secondList = subWeb.GetList(SPUtility.ConcatUrls(subWeb.ServerRelativeUrl, Url));
                    Assert.IsNotNull(secondList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, secondList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// When EnsureList is used with a web-relative url (for example, "testurl"), and a sub-site already exists with the
        /// same relative url, it should throw an exception because of a Url conflict.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_TryingToEnsureAListWithRelativeUrlCorrespondingToSubSiteUrl_ShouldThrowException()
        {
            const string Url = "testUrl";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // First, create the subweb
                var rootWeb = testScope.SiteCollection.RootWeb;
                var subWeb = rootWeb.Webs.Add(Url);

                // Now, attempt to create the list which should result in a conflicting relative Url, thus, an exception thrown.
                var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    IListHelper listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    SPList list = listHelper.EnsureList(rootWeb, listInfo);

                    // Asserting that the list wasn't created
                    Assert.AreEqual(numberOfListsOnRootWebBefore, rootWeb.Lists.Count);
                    Assert.IsNull(list);
                }
            }
        }

        #endregion
    }
}
