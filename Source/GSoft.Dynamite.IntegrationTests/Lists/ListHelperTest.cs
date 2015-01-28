using System;
using System.Collections.Generic;
using System.Globalization;
using Autofac;
using GSoft.Dynamite.Binding;
using GSoft.Dynamite.ContentTypes;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists;
using Microsoft.SharePoint;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SharePoint.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Web.Hosting.Administration;

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
            // Arrange
            const string Url = "Lists/testUrl";

            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo(Url, "nameKey", "descriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;
                    var numberOfListsBefore = testRootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(testRootWeb, listInfo);

                    // Assert
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
            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo("testUrl", "nameKey", "descriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;
                    var numberOfListsBefore = testRootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(testRootWeb, listInfo);

                    // Assert
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
            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                var listInfo = new ListInfo("testUrl", "nameKey", "descriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;

                    // 1- Create the list
                    var numberOfListsBefore = testRootWeb.Lists.Count;
                    var list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    
                    var newlyCreatedList = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);

                    // Act
                    // 2- Ensure the list a second time, now that it's been created
                    var expectingListCreatedAtStep1 = listHelper.EnsureList(testRootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(list.ID, expectingListCreatedAtStep1.ID);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, expectingListCreatedAtStep1.TitleResource.Value);
                    Assert.AreEqual(listInfo.DescriptionResourceKey, expectingListCreatedAtStep1.DescriptionResource.Value);

                    var listCreatedAtStep1 = testRootWeb.GetList("testUrl");
                    Assert.IsNotNull(listCreatedAtStep1);
                    Assert.AreEqual(list.ID, listCreatedAtStep1.ID);
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
        public void EnsureList_WhenListWithSameNameExistsButDifferentURL_ShouldThrowException()
        {
            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                const string SameNameKey = "nameKey";
                const string SameDescriptionKey = "descriptionKey";
                const string Url = "testUrl";
                const string SecondUrl = "Lists/" + Url;
                var listInfo = new ListInfo(Url, SameNameKey, SameDescriptionKey);
                var secondListInfo = new ListInfo(SecondUrl, SameNameKey, SameDescriptionKey);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var testRootWeb = testScope.SiteCollection.RootWeb;

                    // 1- Create (by EnsureList) a first list at "testUrl"
                    var numberOfListsBefore = testRootWeb.Lists.Count;
                    var list = listHelper.EnsureList(testRootWeb, listInfo);

                    Assert.AreEqual(numberOfListsBefore + 1, testRootWeb.Lists.Count);
                    Assert.IsNotNull(list);

                    var newlyCreatedList = testRootWeb.GetList(Url);
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);

                    // Act
                    // 2- Now, attempt to create a list with the same name at a different URL ("/Lists/secondUrl")
                    var secondListShouldThrowException = listHelper.EnsureList(testRootWeb, secondListInfo);

                    // Assert
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
        public void EnsureList_ListDoesntExistAndWantToCreateOnASubWebOneLevelUnderRoot_ShouldCreateAtCorrectURL()
        {
            // Arrange
            const string Url = "some/random/path";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Creating the ListInfo and the sub-web
                var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
                var subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnSubWebBefore = subWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(subWeb, listInfo);

                    // Assert
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
            // Arrange
            const string Url = "testUrl";
            const string NameKey = "NameKey";
            const string DescKey = "DescriptionKey";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Let's first create a list on the root web
                var listInfo = new ListInfo(Url, NameKey, DescKey);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var rootWeb = testScope.SiteCollection.RootWeb;
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    var listRootWeb = listHelper.EnsureList(rootWeb, listInfo);
                    
                    Assert.AreEqual(numberOfListsOnRootWebBefore + 1, rootWeb.Lists.Count);
                    Assert.IsNotNull(listRootWeb);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listRootWeb.TitleResource.Value);

                    // Now let's create a sub web under root, and try to ensure the "same" list there. It should create a new one.
                    var subWeb = rootWeb.Webs.Add("subweb");
                    var numberOfListsOnSubWebBefore = subWeb.Lists.Count;

                    // Act
                    var listSubWeb = listHelper.EnsureList(subWeb, listInfo);
                    
                    // Assert
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
        /// When EnsureList is used with a web-relative URL (for example, "testurl"), and a sub-site already exists with the
        /// same relative URL, it should throw an exception because of a URL conflict.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_TryingToEnsureAListWithRelativeURLCorrespondingToSubSiteURL_ShouldThrowException()
        {
            // Arrange
            const string Url = "testUrl";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // First, create the subweb
                var rootWeb = testScope.SiteCollection.RootWeb;
                var subWeb = rootWeb.Webs.Add(Url);

                // Now, attempt to create the list which should result in a conflicting relative URL, thus, an exception thrown.
                var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Asserting that the list wasn't created
                    Assert.AreEqual(numberOfListsOnRootWebBefore, rootWeb.Lists.Count);
                    Assert.IsNull(list);
                }
            }
        }

        /// <summary>
        /// When EnsureList is used with a web-relative URL (for example, "testurl"), and a folder already exists with the
        /// same relative URL, it should throw an exception because of a URL conflict.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_TryingToEnsureAListWithRelativeURLCorrespondingToAFolder_ShouldThrowException()
        {
            // Arrange
            const string Url = "testUrl";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // First, create the folder
                var rootWeb = testScope.SiteCollection.RootWeb;
                var folder = rootWeb.RootFolder.SubFolders.Add(Url);

                // Now, attempt to create a list which should result in a conflicting relative URL and a thrown exception.
                var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    // Act
                    var listThatShouldNotBeCreated = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert (list should not have been created, and an exception thrown)
                    Assert.AreEqual(numberOfListsOnRootWebBefore, rootWeb.Lists.Count);
                    Assert.IsNull(listThatShouldNotBeCreated);
                }
            }
        }

        #endregion

        #region Make sure everything works fine when using sites managed paths

        /// <summary>
        /// Making sure EnsureList works fine when site collection is on a managed path.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenListDoesntExistTryingToCreateOnSiteManagedPath_ShouldCreateList()
        {
            // Arrange
            const string ManagedPath = "managed";
            const string ListUrl = "some/random/path";

            using (var testScope = SiteTestScope.ManagedPathSite(ManagedPath))
            {
                var rootWeb = testScope.SiteCollection.RootWeb;
                var listInfo = new ListInfo(ListUrl, "NameKey", "DescriptionKey");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsOnRootWebBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsOnRootWebBefore + 1, rootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);

                    // Fetching the list, to make sure it persists on the web
                    var newlyCreatedList = rootWeb.GetList(ManagedPath + "/" + ListUrl);
                    Assert.IsNotNull(newlyCreatedList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, newlyCreatedList.TitleResource.Value);
                }
            }
        }

        /// <summary>
        /// Case when trying to create a list with a path/URL that's already taken by a site managed path.
        /// It should throw an exception. For example, trying to create a list with URL "managed" under the root web
        /// of a site located at server-relative path "/", but the "managed" path is already taken by a site managed path.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_WhenPathReservedForSiteManagedPathTryingToBeUsedAsAListPath_ShouldThrowException()
        {
            // Arrange
            const string ManagedPath = "managed";
            var listInfo = new ListInfo(ManagedPath, "NameKey", "DescriptionKey");

            using (var managedPathSite = SiteTestScope.ManagedPathSite(ManagedPath))
            {
                using (var siteAtServerRoot = SiteTestScope.BlankSite())
                {
                    var rootSiteRootWeb = siteAtServerRoot.SiteCollection.RootWeb;
                    var numberOfListsOnRootSiteRootWeb = rootSiteRootWeb.Lists.Count;

                    using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                    {
                        var listHelper = injectionScope.Resolve<IListHelper>();

                        // Act
                        var list = listHelper.EnsureList(rootSiteRootWeb, listInfo);

                        // Assert (exception expected)
                        Assert.AreEqual(numberOfListsOnRootSiteRootWeb, rootSiteRootWeb.Lists.Count);
                        Assert.IsNull(list);
                    }
                }
            }
        }

        #endregion

        #region Make sure EnsureList updates and/or applies correctly the different properties of a list (and make sure overwrite works fine)

        /// <summary>
        /// In the case the list already exists (based on the URL), and Overwrite property is at true,
        /// EnsureList should delete the existing list and create a new one.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenListExistsBasedOnURLAndOverwriteIsTrue_ItShouldRecreateTheList()
        {
            // Arrange (create the initial list so we can test the overwrite property)
            const string Url = "testUrl";
            const string NameKey = "nameKey";
            const string DescKey = "DescriptionKey";
            var listInfo = new ListInfo(Url, NameKey, DescKey);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);
                    var initialListID = initialList.ID;

                    // Making sure the initial list got created
                    Assert.IsNotNull(initialList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, initialList.TitleResource.Value);
                    initialList = rootWeb.GetList(Url);
                    Assert.IsNotNull(initialList);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, initialList.TitleResource.Value);

                    var oneEmptyItem = initialList.Items.Add();
                    oneEmptyItem.Update();

                    // Setting up the second list info
                    var listInfoForOverwrite = new ListInfo(Url, "SecondList", "DescSecondList");
                    listInfoForOverwrite.Overwrite = true;

                    // Act
                    var secondList = listHelper.EnsureList(rootWeb, listInfoForOverwrite);

                    // Assert
                    Assert.IsNotNull(secondList);
                    Assert.AreEqual(listInfoForOverwrite.DisplayNameResourceKey, secondList.TitleResource.Value);
                    secondList = rootWeb.GetList(Url);
                    Assert.IsNotNull(secondList);
                    Assert.AreEqual(listInfoForOverwrite.DisplayNameResourceKey, secondList.TitleResource.Value);
                    Assert.AreEqual(listInfoForOverwrite.DescriptionResourceKey, secondList.DescriptionResource.Value);

                    Assert.AreNotEqual(initialListID, secondList.ID);
                }
            }
        }

        /// <summary>
        /// Using EnsureList to update the name of an existing list. The item already created on the list, and everything else
        /// besides the name should stay the same.
        /// </summary>
        [TestMethod]
        public void EnsureList_ExistingListWithItemCreatedThenEnsuringThatSameListToUpdateName_ShouldKeepSameListWithUpdatedName()
        {
            // Arrange
            const string Url = "testUrl";
            const string DescKey = "DescriptionKey";
            var listName = "InitialName";

            var listInfo = new ListInfo(Url, listName, DescKey);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;
                var numberOfListsBefore = rootWeb.Lists.Count;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    
                    // Creating the list and adding one item in it
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);
                    Assert.IsNotNull(initialList);
                    var item = initialList.AddItem();
                    item["Title"] = "Item Title";
                    item.Update();

                    var listWithItem = rootWeb.GetList(Url);
                    Assert.IsNotNull(listWithItem);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listWithItem.TitleResource.Value);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listWithItem.ItemCount, 1);

                    // Act
                    var updatedListInfo = new ListInfo(Url, "Test_ListDisplayName", DescKey);
                    var expectedDisplayName = "EN List Name";
                    var updatedList = listHelper.EnsureList(rootWeb, updatedListInfo);

                    // Assert
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.AreEqual(expectedDisplayName, updatedList.TitleResource.Value);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listWithItem.ItemCount, 1);
                    Assert.AreEqual(updatedList.Items[0]["Title"], "Item Title");

                    Assert.AreEqual(initialList.ID, updatedList.ID);
                }
            }
        }

        /// <summary>
        /// Using EnsureList to update the description of an existing list. The item already created on the list, and everything else
        /// besides the description should stay the same.
        /// </summary>
        [TestMethod]
        public void EnsureList_ExistingListWithItemCreatedThenEnsuringThatSameListToUpdateDescription_ShouldKeepSameListWithUpdatedDesc()
        {
            // Arrange
            const string Url = "testUrl";
            const string ListName = "NameKey";
            var initialDescription = "Initial Description";      

            var listInfo = new ListInfo(Url, ListName, initialDescription);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;
                var numberOfListsBefore = rootWeb.Lists.Count;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Creating the list and adding one item in it
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);
                    Assert.IsNotNull(initialList);
                    var item = initialList.AddItem();
                    item["Title"] = "Item Title";
                    item.Update();

                    var listWithItem = rootWeb.GetList(Url);
                    Assert.IsNotNull(listWithItem);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, listWithItem.TitleResource.Value);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listWithItem.ItemCount, 1);

                    // Act
                    var updatedListInfo = new ListInfo(Url, ListName, "Test_ListDescription");
                    var expectedDescription = "EN List Description";
                    var updatedList = listHelper.EnsureList(rootWeb, updatedListInfo);

                    // Assert
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.AreEqual(expectedDescription, updatedList.DescriptionResource.Value);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listWithItem.ItemCount, 1);
                    Assert.AreEqual(updatedList.Items[0]["Title"], "Item Title");

                    Assert.AreEqual(initialList.ID, updatedList.ID);
                }
            }
        }

        /// <summary>
        /// The option "RemoveDefaultContentType" should delete the Item content type when ensuring a list.
        /// In this case, the list will created with this option set to true.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenCreatingANewListAndWeWantToRemoveTheItemContentType_ShouldRemoveIt()
        {
            // Arrange
            const string Url = "testUrl";
            const string NameKey = "Name Key";
            const string DescKey = "DescriptionKey";
            var listInfo = new ListInfo(Url, NameKey, DescKey);
            listInfo.RemoveDefaultContentType = true;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    
                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.IsNotNull(list);

                    // Fetch the list to make sure it persisted on the web
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);

                    // Check the number of content types associated to the newly created custom list
                    // The default is 2 (Item and Folder).... expecting only one since Item should not be there.
                    Assert.AreEqual(list.ContentTypes.Count, 1);
                    Assert.IsNull(list.ContentTypes["Item"]);
                    Assert.IsNotNull(list.ContentTypes["Folder"]);
                }
            }
        }

        /// <summary>
        /// A custom list is first created with property "RemoveDefaultContentType" set to false. Then,
        /// anoter EnsureList is called on the same list, with the property now set to true. The content type
        /// "Item" is expected to be removed.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenUpdatingAListWithAContentTypeItemWithRemoveDefaultContentTypeOn_ShouldDeleteItemContentType()
        {
            // Arrange
            const string Url = "testUrl";
            const string NameKey = "Name Key";
            const string DescKey = "DescriptionKey";
            var listInfo = new ListInfo(Url, NameKey, DescKey);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    Assert.IsNotNull(list);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.AreEqual(list.ContentTypes.Count, 2);
                    Assert.IsNotNull(list.ContentTypes["Item"]);

                    var numberOfListsBefore = rootWeb.Lists.Count;
                    var listInfoRemoveItemContentType = new ListInfo(Url, NameKey, DescKey);
                    listInfoRemoveItemContentType.RemoveDefaultContentType = true;

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfoRemoveItemContentType);

                    // Assert
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBefore, rootWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    Assert.AreEqual(updatedList.ContentTypes.Count, 1);
                    Assert.IsNull(updatedList.ContentTypes["Item"]);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring a list that already exists and contains one or more items using de Item content type,
        /// and trying to remove that content type with the property RemoveDefaultContentType, it should throw an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SPException))]
        public void EnsureList_WhenEnsuringExistingListWithRemoveDefContTypeButItemsUsingIt_ShouldThrowException()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    Assert.IsNotNull(list);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.AreEqual(list.ContentTypes.Count, 2);
                    Assert.IsNotNull(list.ContentTypes["Item"]);

                    // Creating an item on the list
                    var item = list.AddItem();
                    item["Title"] = "Item Title";
                    item.Update();

                    // Act
                    listInfo.RemoveDefaultContentType = true;
                    var listAfter = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert (exception should have been thrown...)
                    Assert.IsTrue(false);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring a non-existant list (so when creating a list) with the property HasDraftVisibilityType
        /// set to true, the list should be set to EnableModeration = true and DraftVersionVisibility to Reader (Value = 0).
        /// </summary>
        [TestMethod]
        public void EnsureList_CreatingAListWithADraftVisibilityType_ShouldCreateListWithProperType()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.HasDraftVisibilityType = true;

            var expectedDraftVersionVisibility = DraftVisibilityType.Reader;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.IsNotNull(list);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.IsTrue(list.EnableModeration);
                    Assert.AreEqual(list.DraftVersionVisibility, expectedDraftVersionVisibility);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring an existing list (so when updating a list) with the property HasDraftVisibilityType
        /// set to true, the list should be set to EnableModeration = true. The DraftVisibilityType will be set to Approver.
        /// </summary>
        [TestMethod]
        public void EnsureList_UpdatingAListWithADraftVisibilityType_ShouldUpdateListWithProperType()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            var expectedDraftVisibilityType = DraftVisibilityType.Approver;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Creating the initial list with EnableModeration = false (Default value)
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);

                    // Creating an item on the list
                    var item = initialList.AddItem();
                    item["Title"] = "Item Title";
                    item.Update();

                    // Act
                    listInfo.HasDraftVisibilityType = true;
                    listInfo.DraftVisibilityType = DraftVisibilityType.Approver;
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    updatedList = rootWeb.GetList(Url);
                    Assert.AreEqual(updatedList.ItemCount, 1);
                    Assert.AreEqual(updatedList.Items[0]["Title"], "Item Title");
                    Assert.IsTrue(updatedList.EnableModeration);
                    Assert.AreEqual(updatedList.DraftVersionVisibility, expectedDraftVisibilityType);
                }
            }
        }

        /// <summary>
        /// Make sure that when ensuring a non-existant list, with property EnableRatings set to true, and with a valid
        /// RatingType specified, the list is properly created (needs a publishing site).
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANonExistingListWithEnableRatingsAndARatingType_ShouldEnableRatingsWithThatType()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.EnableRatings = true;
            listInfo.RatingType = "Ratings";

            using (var testScope = SiteTestScope.PublishingSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.IsNotNull(list);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(list.RootFolder.Properties["Ratings_VotingExperience"], "Ratings");
                }
            }
        }

        /// <summary>
        /// Make sure that when ensuring an existing list, with property EnableRatings set to true, and with a valid
        /// RatingType specified, the list is properly updated (needs a publishing site).
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringAnExistingListAndWantToEnableRatingsWithProperType_ShouldKeepListAndEnableRatingsOnIt()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            using (var testScope = SiteTestScope.PublishingSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);

                    // Make sure everything was correctly set up
                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;

                    // Ratings are not enabled yet
                    Assert.AreEqual(initialList.RootFolder.Properties["Ratings_VotingExperience"], string.Empty);

                    // Prepare the update
                    listInfo.EnableRatings = true;
                    listInfo.RatingType = "Likes";

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    Assert.AreEqual(updatedList.RootFolder.Properties["Ratings_VotingExperience"], "Likes");
                }
            }
        }

        /// <summary>
        /// Make sure that when ensuring a list, with property EnableRatings set to true, and with a valid
        /// RatingType specified, but with a non-valid site, i.e, not a publishing site, an exception is thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SPException))]
        public void EnsureList_WhenTryingToEnableRatingsButTheSiteIsNotOfTypePublishing_ShouldThrowAnException()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.EnableRatings = true;
            listInfo.RatingType = "Ratings";

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert (an exception should have been thrown...)
                    Assert.IsTrue(false);
                }
            }
        }

        /// <summary>
        /// Make that when Ensuring a new list, if the WriteSecurity property specifies a value (default is 1 - all users),
        /// the list is created with the correct value.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListAndWithAWriteSecuritySpecified_ItShouldCreateTheListWithTheRightPermission()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            // OwnerOnly = 2
            listInfo.WriteSecurity = WriteSecurityOptions.OwnerOnly;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(list.WriteSecurity, 2);
                }
            }
        }

        /// <summary>
        /// Make that when Ensuring an existing list, if the WriteSecurity property specifies a value (default is 1 - all users),
        /// the list is updated with the correct value.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringAnExistingListAndWithAWriteSecuritySpecified_ItShouldUpdateTheListWithTheRightPermission()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;

                    // Nobody = 4
                    listInfo.WriteSecurity = WriteSecurityOptions.Nobody;

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    updatedList = rootWeb.GetList(Url);
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.AreEqual(updatedList.WriteSecurity, 4);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring a new list and the AddToQuickLaunch property is set to true,
        /// a shortcut to the list is added to quick launch.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListWithAddToQuickLauchSetToTrue_ItShouldAddTheNewListToQuickLaunch()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.AddToQuickLaunch = true;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;
                    var pathToListsInQuickLaunch = rootWeb.Navigation.QuickLaunch[0].Children;
                    var numberOfItemsOnListsQuickLaunch = pathToListsInQuickLaunch.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Get the updated nav bar
                    pathToListsInQuickLaunch = rootWeb.Navigation.QuickLaunch[0].Children;

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.IsTrue(list.OnQuickLaunch);
                    Assert.AreEqual(numberOfItemsOnListsQuickLaunch + 1, pathToListsInQuickLaunch.Count);
                    Assert.AreEqual(pathToListsInQuickLaunch[0].Title, listInfo.DisplayNameResourceKey);
                }
            }
        }

        /// <summary>
        /// Make that when Ensuring an existing list, if the AddToQuickLaunch property specifies is set to true,
        /// the list is added to quick launch bar.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringAnExistingListAndWithAddToQuickLaunchToTrue_ItShouldUpdateTheQuickLaunchBar()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;

                    listInfo.AddToQuickLaunch = true;

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);
                    var pathToListsInQuickLaunch = rootWeb.Navigation.QuickLaunch[0].Children;

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    updatedList = rootWeb.GetList(Url);
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.IsTrue(updatedList.OnQuickLaunch);
                    Assert.AreEqual(pathToListsInQuickLaunch.Count, 1);
                    Assert.AreEqual(pathToListsInQuickLaunch[0].Title, listInfo.DisplayNameResourceKey);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring a new list and the EnableAttachments property is set to false,
        /// it is disabled.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListWithEnableAttachmentsSetToFalse_ItShouldDisableThem()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.EnableAttachements = false;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.IsFalse(list.EnableAttachments);
                }
            }
        }

        /// <summary>
        /// Case where you have an existing list with attachments disabled. You want to ensure/update it
        /// to enable attachments.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringAnExistingListWithEnableAttachmentsSetToFalse_ItShouldEnableThem()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");
            listInfo.EnableAttachements = false;

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;
                    Assert.IsFalse(list.EnableAttachments);

                    listInfo.EnableAttachements = true;

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    updatedList = rootWeb.GetList(Url);
                    Assert.IsNotNull(updatedList);
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.IsTrue(updatedList.EnableAttachments);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring an existing list with item(s) on it, and the new list info has
        /// property EnableAttachments set to false, it doesn't allow you to disable them to prevent from deleting attachments.
        /// An ArgumentException is thrown.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EnsureList_WhenEnsuringAnExistingListWithItemsOnItAndYouWantToDisableAttachments_ItShouldNotAllowYou()
        {
            // Arrange
            const string Url = "testUrl";
            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var initialList = listHelper.EnsureList(rootWeb, listInfo);

                    // Creating an item on the list
                    var item = initialList.AddItem();
                    item["Title"] = "Item Title";
                    item.Update();

                    listInfo.EnableAttachements = false;

                    // Act
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert (exception should have been thrown...)
                    Assert.IsTrue(false);
                }
            }
        }

        #endregion

        #region Make sure fields definitions defined on ListInfo are correctly created and saved on a list when ensuring that list.

        /// <summary>
        /// Make sure that when Ensuring a new list with a field definitions specified, those fields are applied to the list
        /// and persisted on the web.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenCreatingANewListWithSpecifiedFieldDefinitions_ItShouldAddAndPersistThem()
        {
            // Arrange
            const string Url = "testUrl";

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            const string Desc = "DescriptionFieldKey";
            const string Name = "NameFieldKey";
            var textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    Name,
                    Desc,
                    "GroupKey");

            listInfo.FieldDefinitions.Add(textFieldInfo);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    list = rootWeb.GetList(Url);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.IsNotNull(list.Fields[Name]);
                    Assert.AreEqual(list.Fields[Name].DescriptionResource.Value, Desc);
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring an existing list, and you have a field definitions specified, 
        /// those fields are applied to the list and persisted on the web.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenUpdatingAListWithSpecifiedFieldDefinitions_ItShouldAddAndPersistThem()
        {
            // Arrange
            const string Url = "testUrl";

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            const string Desc = "DescriptionFieldKey";
            const string Name = "NameFieldKey";
            var textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    Name,
                    Desc,
                    "GroupKey");

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;
                    var numberOfFieldsOnListBefore = list.Fields.Count;
                    var numberOfSiteColumnsBefore = rootWeb.Fields.Count;

                    // Act
                    listInfo.FieldDefinitions.Add(textFieldInfo);
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    Assert.AreEqual(updatedList.Fields.Count, numberOfFieldsOnListBefore + 1);
                    Assert.AreEqual(numberOfSiteColumnsBefore + 1, rootWeb.Fields.Count);
                    Assert.IsNotNull(updatedList.Fields[Name]);
                    Assert.AreEqual(updatedList.Fields[Name].DescriptionResource.Value, Desc);
                }
            }
        }

        #endregion

        #region Default view fields should be updated during ensure

        /// <summary>
        /// Make sure that when Ensuring a new list with a field collection specified, the field(s) added to the Default View
        /// property should correctly be added to the view.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListAndAddingAFieldAndWantItToBeInDefaultView_ShouldBeAddedToDefaultView()
        {
            // Arrange
            const string Url = "testUrl";

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            const string Desc = "DescriptionFieldKey";
            const string Name = "NameFieldKey";
            var textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    Name,
                    Desc,
                    "GroupKey");

            listInfo.FieldDefinitions.Add(textFieldInfo);
            listInfo.DefaultViewFields.Add(textFieldInfo);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.IsNotNull(list);
                    list = rootWeb.GetList(Url);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    Assert.IsNotNull(list.Fields[Name]);
                    Assert.AreEqual(list.Fields[Name].DescriptionResource.Value, Desc);
                    Assert.IsTrue(list.DefaultView.ViewFields.Exists("TestInternalName"));
                }
            }
        }

        /// <summary>
        /// Make sure that when Ensuring an existing list with a field collection to be added to the Default View
        /// specified, thoses fields should be correctly added to the view.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenUpdatingAListToAddFieldsToDefaultView_TheDefaultViewShouldBeModifiedAccordingly()
        {
            // Arrange
            const string Url = "testUrl";

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey");

            const string Desc = "DescriptionFieldKey";
            const string Name = "NameFieldKey";
            var textFieldInfo = new TextFieldInfo(
                    "TestInternalName",
                    new Guid("{0C58B4A1-B360-47FE-84F7-4D8F58AE80F6}"),
                    Name,
                    Desc,
                    "GroupKey");

            var numberFieldInfo = new NumberFieldInfo(
                    "NumberInternalName",
                    new Guid("{953D865E-7C19-4961-9643-1BFCE3AC3889}"),
                    "NameKey2",
                    "DescKey2",
                    "GroupKey2");

            listInfo.FieldDefinitions.Add(textFieldInfo);
            listInfo.FieldDefinitions.Add(numberFieldInfo);

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var numberOfListsBeforeUpdate = rootWeb.Lists.Count;

                    // Act
                    listInfo.DefaultViewFields.Add(numberFieldInfo);
                    var updatedList = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBeforeUpdate, rootWeb.Lists.Count);
                    Assert.IsNotNull(updatedList);
                    updatedList = rootWeb.GetList(Url);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, updatedList.TitleResource.Value);
                    Assert.IsNotNull(updatedList.Fields[Name]);
                    Assert.IsNotNull(updatedList.Fields["NameKey2"]);
                    Assert.IsTrue(list.DefaultView.ViewFields.Exists("NumberInternalName"));
                }
            }
        }

        #endregion 

        #region Ensuring content types on a list should provision the content types properly

        /// <summary>
        /// Make sure that when you use List Info to ensure a list, and you specifiy a content type collection,
        /// the list is created with those content types and of course they are enabled.
        /// In this case, if the content types didn't already exist on the root web, it should be ensured there
        /// beforehand, as a convenience/bit of magic. We want to enforce the convention of "don't define CTs
        /// directly on list, re-use site collection CTs instead".
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListInRootWebWithSpecifiedContentTypes_AndContentTypeDoesntExistOnRootWebYet_ItShouldAddThemToBothRootWebAndList()
        {
            // Arrange
            const string Url = "testUrl";
            
            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey");

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
                {
                    ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo
                    }
                };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = rootWeb.Lists.Count;
                    var numberOfContentTypesBefore = rootWeb.ContentTypes.Count;
                    
                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);
                    Assert.IsTrue(list.ContentTypesEnabled);

                    // Make sure CT was provisionned on root web and that list CT is child of root web CT
                    Assert.AreEqual(numberOfContentTypesBefore + 1, rootWeb.ContentTypes.Count);
                    var rootWebContentType = rootWeb.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(rootWebContentType);
                    Assert.AreEqual(contentTypeInfo.DisplayNameResourceKey, rootWebContentType.NameResource.Value);

                    Assert.IsTrue(listContentType.Id.IsChildOf(rootWebContentType.Id));
                }
            }
        }

        /// <summary>
        /// Make sure that when you use List Info to ensure a list, and you specifiy a content type collection,
        /// the list is created with those content types and of course they are enabled.
        /// In this case, if the content types already existed on the root web, the list content types should
        /// be created as children on the root web content type.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListInRootWebWithSpecifiedContentTypes_AndContentTypeAlreadyExistsOnRootWeb_ItShouldAddChildOfRootWebContentTypeToList()
        {
            // Arrange
            const string Url = "testUrl";

            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey");

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Add the content type to the root web beforehand
                    contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    var numberOfListsBefore = rootWeb.Lists.Count;
                    var numberOfContentTypesBefore = rootWeb.ContentTypes.Count;

                    // Act
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, rootWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = rootWeb.GetList(Url);
                    Assert.IsNotNull(list);
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);

                    // Make sure CT was not re-provisionned on root web and that list CT is child of root web CT
                    Assert.AreEqual(numberOfContentTypesBefore, rootWeb.ContentTypes.Count);    // same number before/after because initial count was take after CT was ensured on root web
                    var rootWebContentType = rootWeb.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(rootWebContentType);

                    Assert.IsTrue(listContentType.Id.IsChildOf(rootWebContentType.Id));
                }
            }
        }

        /// <summary>
        /// Make sure that when you use List Info to ensure a list on a sub-web, and you specifiy a content type collection,
        /// the list is created with those content types and of course they are enabled.
        /// In this case, if the content types already existed on the root web, the list content types should
        /// be created as children on the root web content type.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListInSubWeWithSpecifiedContentTypes_AndContentTypeDoesntExistOnRootWebYet_ItShouldAddThemToBothRootWebAndList()
        {
            // Arrange
            const string Url = "testUrl";

            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey");

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;
                var subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var numberOfListsBefore = subWeb.Lists.Count;
                    var numberOfContentTypesBefore = rootWeb.ContentTypes.Count;

                    // Act
                    var list = listHelper.EnsureList(subWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, subWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = subWeb.GetList("subweb/" + Url);
                    Assert.IsNotNull(list);
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);
                    Assert.IsTrue(list.ContentTypesEnabled);

                    // Make sure CT was provisionned on root web and that list CT is child of root web CT
                    Assert.AreEqual(numberOfContentTypesBefore + 1, rootWeb.ContentTypes.Count);
                    var rootWebContentType = rootWeb.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(rootWebContentType);
                    Assert.AreEqual(contentTypeInfo.DisplayNameResourceKey, rootWebContentType.NameResource.Value);

                    Assert.IsTrue(listContentType.Id.IsChildOf(rootWebContentType.Id));
                }
            }
        }

        /// <summary>
        /// Make sure that when you use List Info to ensure a list on a sub-web, and you specifiy a content type collection,
        /// the list is created with those content types and of course they are enabled.
        /// In this case, if the content types already existed on the root web, the list content types should
        /// be created as children on the root web content type.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenEnsuringANewListInSubWebWithSpecifiedContentTypes_AndContentTypeAlreadyExistsOnRootWeb_ItShouldAddChildOfRootWebContentTypeToList()
        {
            // Arrange
            const string Url = "testUrl";

            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey");

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;
                var subWeb = testScope.SiteCollection.RootWeb.Webs.Add("subweb");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Add the content type to the root web beforehand
                    contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    var numberOfListsBefore = subWeb.Lists.Count;
                    var numberOfContentTypesBefore = rootWeb.ContentTypes.Count;

                    // Act
                    var list = listHelper.EnsureList(subWeb, listInfo);

                    // Assert
                    Assert.AreEqual(numberOfListsBefore + 1, subWeb.Lists.Count);
                    Assert.AreEqual(listInfo.DisplayNameResourceKey, list.TitleResource.Value);
                    list = subWeb.GetList("subweb/" + Url);
                    Assert.IsNotNull(list);
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);

                    // Make sure CT was not re-provisionned on root web and that list CT is child of root web CT
                    Assert.AreEqual(numberOfContentTypesBefore, rootWeb.ContentTypes.Count);    // same number before/after because initial count was take after CT was ensured on root web
                    var rootWebContentType = rootWeb.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(rootWebContentType);

                    Assert.IsTrue(listContentType.Id.IsChildOf(rootWebContentType.Id));
                }
            }
        }

        #endregion

        #region When both Content Types and Field Definitions are specified, field definitions should be used to update the list fields (and site columns and content types should get ensured, if needed)

        /// <summary>
        /// Validates that field definitions can override the CT's original field definitions.
        /// This is the case where the site column and site content type hadn't been deployed yet (the ListHelper should auto-magically
        /// provision those for you)
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenSpecifyingBothContentTypesAndFieldDefinitions_AndNoneOfThemAreProvisionnedYet_ShouldProvisionSiteColumnAndContentType_ThenOverrideFieldDefinitionOnTheList()
        {
            // Arrange
            const string Url = "testUrl";
            
            var testFieldInfoRequired = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
                {
                    Required = RequiredType.Required
                };

            var sameTestFieldInfoButNotRequired = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
                {
                    Required = RequiredType.NotRequired
                };

            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey")
                {
                    Fields = new List<IFieldInfo>()
                    {
                        testFieldInfoRequired
                    }
                };

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo     // this includes testFieldInfoRequired 
                    },
                FieldDefinitions = new List<IFieldInfo>()
                    {
                        sameTestFieldInfoButNotRequired     // this is the list-specific field definition override
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                                        
                    // Act: Provision the list with an alternate field definition (NotRequired)
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var listRefetched = rootWeb.Lists[list.ID];

                    // Assert

                    // Site column should've been added to root web
                    var siteColumn = rootWeb.Fields[testFieldInfoRequired.Id];
                    Assert.IsNotNull(siteColumn);
                    Assert.IsTrue(siteColumn.Required);

                    // Content type should've been added to root web
                    var siteContentType = rootWeb.ContentTypes[new SPContentTypeId(contentTypeId)];
                    Assert.IsNotNull(siteContentType);
                    Assert.IsNotNull(siteContentType.Fields[testFieldInfoRequired.Id]);

                    // Content type should've been added to new list
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);
                    Assert.IsNotNull(listContentType.Fields[testFieldInfoRequired.Id]);

                    var listContentTypeRefetched = rootWeb.Lists[list.ID].ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentTypeRefetched);
                    Assert.IsNotNull(listContentTypeRefetched.Fields[testFieldInfoRequired.Id]);

                    // New list should hold a NotRequired variant of the site column
                    var listColumn = list.Fields[sameTestFieldInfoButNotRequired.Id];
                    Assert.IsFalse(listColumn.Required);

                    var listColumnRefetched = rootWeb.Lists[list.ID].Fields[sameTestFieldInfoButNotRequired.Id];
                    Assert.IsFalse(listColumnRefetched.Required);    // definition should've been overriden for the list only
                }
            }
        }

        /// <summary>
        /// Validates that field definitions can override the CT's original field definitions.
        /// This is the case where the site column and site content type had already been deployed.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenSpecifyingBothContentTypesAndFieldDefinitions_AndSiteColumnAndContenTypeAlreadyThere_ShouldProvisionListContentType_ThenOverrideFieldDefinitionOnTheList()
        {
            // Arrange
            const string Url = "testUrl";

            var testFieldInfoRequired = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
            {
                Required = RequiredType.Required
            };

            var sameTestFieldInfoButNotRequired = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
            {
                Required = RequiredType.NotRequired
            };

            var contentTypeId = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeInfo = new ContentTypeInfo(contentTypeId, "ContentTypeNameKey", "ContentTypeDescKey", "GroupKey")
            {
                Fields = new List<IFieldInfo>()
                    {
                        testFieldInfoRequired
                    }
            };

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo     // this includes testFieldInfoRequired 
                    },
                FieldDefinitions = new List<IFieldInfo>()
                    {
                        sameTestFieldInfoButNotRequired     // this is the list-specific field definition override
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    var fieldHelper = injectionScope.Resolve<IFieldHelper>();
                    var contentTypeHelper = injectionScope.Resolve<IContentTypeHelper>();

                    // Provision the site column and site content type first
                    var siteColumn = fieldHelper.EnsureField(rootWeb.Fields, testFieldInfoRequired);
                    var siteContentType = contentTypeHelper.EnsureContentType(rootWeb.ContentTypes, contentTypeInfo);

                    // Act: Provision the list with an alternate field definition (NotRequired)
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var listRefetched = rootWeb.Lists[list.ID];

                    // Assert

                    // Site column should've been added to root web
                    var siteColumnRefetched = rootWeb.Fields[testFieldInfoRequired.Id];
                    Assert.IsNotNull(siteColumnRefetched);
                    Assert.IsTrue(siteColumnRefetched.Required);

                    // Content type should've been added to root web
                    var siteContentTypeRefetched = rootWeb.ContentTypes[new SPContentTypeId(contentTypeId)];
                    Assert.IsNotNull(siteContentTypeRefetched);
                    Assert.IsNotNull(siteContentTypeRefetched.Fields[testFieldInfoRequired.Id]);

                    // Content type should've been added to new list
                    var listContentType = list.ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentType);
                    Assert.IsNotNull(listContentType.Fields[testFieldInfoRequired.Id]);

                    var listContentTypeRefetched = rootWeb.Lists[list.ID].ContentTypes["ContentTypeNameKey"];
                    Assert.IsNotNull(listContentTypeRefetched);
                    Assert.IsNotNull(listContentTypeRefetched.Fields[testFieldInfoRequired.Id]);

                    // New list should hold a NotRequired variant of the site column
                    var listColumn = list.Fields[sameTestFieldInfoButNotRequired.Id];
                    Assert.IsFalse(listColumn.Required);

                    var listColumnRefetched = rootWeb.Lists[list.ID].Fields[sameTestFieldInfoButNotRequired.Id];
                    Assert.IsFalse(listColumnRefetched.Required);    // definition should've been overriden for the list only
                }
            }
        }

        #endregion

        #region The UniqueContentTypeOrder should be respected during initial provisionning and updates

        /// <summary>
        /// Validates that the ordering of content types on ListInfo determines the UniqueContentTypeOrder on the list's root folder.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenAddingContentTypesToList_ShouldInitializeUniqueContentTypeOrderToFollowInputOrder()
        {
            // Arrange
            const string Url = "testUrl";

            var testFieldInfo = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
            {
                Required = RequiredType.Required
            };

            var contentTypeId1 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeId2 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{A7BAA5B7-C57B-4928-9778-818D267505A1}"));

            var contentTypeInfo1 = new ContentTypeInfo(contentTypeId1, "ContentTypeNameKey1", "ContentTypeDescKey1", "GroupKey")
            {
                Fields = new List<IFieldInfo>()
                    {
                        testFieldInfo
                    }
            };

            var contentTypeInfo2 = new ContentTypeInfo(contentTypeId2, "ContentTypeNameKey2", "ContentTypeDescKey2", "GroupKey")
            {
                Fields = new List<IFieldInfo>()
                    {
                        testFieldInfo
                    }
            };

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo1,
                        contentTypeInfo2
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();

                    // Act: Provision the list with 2 CTs
                    var list = listHelper.EnsureList(rootWeb, listInfo);
                    var listRefetched = rootWeb.Lists[list.ID];

                    // Assert
                    var listUniqueContentTypeOrder = list.RootFolder.UniqueContentTypeOrder;
                    Assert.IsTrue(listUniqueContentTypeOrder[0].Id.IsChildOf(new SPContentTypeId(contentTypeInfo1.ContentTypeId)));
                    Assert.IsTrue(listUniqueContentTypeOrder[1].Id.IsChildOf(new SPContentTypeId(contentTypeInfo2.ContentTypeId)));

                    var listUniqueContentTypeOrderRefetched = listRefetched.RootFolder.UniqueContentTypeOrder;
                    Assert.IsTrue(listUniqueContentTypeOrderRefetched[0].Id.IsChildOf(new SPContentTypeId(contentTypeInfo1.ContentTypeId)));
                    Assert.IsTrue(listUniqueContentTypeOrderRefetched[1].Id.IsChildOf(new SPContentTypeId(contentTypeInfo2.ContentTypeId)));
                }
            }
        }

        /// <summary>
        /// Validates that the ordering of content types on ListInfo determines the UniqueContentTypeOrder on the list's root folder.
        /// </summary>
        [TestMethod]
        public void EnsureList_WhenUpdateingContentTypesOnList_ShouldUpdateUniqueContentTypeOrderToFollowInputOrder()
        {
            // Arrange
            const string Url = "testUrl";

            var testFieldInfo = new TextFieldInfo("TestFieldInfoText", new Guid("{3AB6C2FB-DA24-4C67-A4CC-7FA4CE07A03F}"), "NameKey", "DescriptionKey", "GroupKey")
            {
                Required = RequiredType.Required
            };

            var contentTypeId1 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{F8B6FF55-2C9E-4FA2-A705-F55FE3D18777}"));

            var contentTypeId2 = string.Format(
                    CultureInfo.InvariantCulture,
                    "0x0100{0:N}",
                    new Guid("{A7BAA5B7-C57B-4928-9778-818D267505A1}"));

            var contentTypeInfo1 = new ContentTypeInfo(contentTypeId1, "ContentTypeNameKey1", "ContentTypeDescKey1", "GroupKey")
            {
                Fields = new List<IFieldInfo>()
                    {
                        testFieldInfo
                    }
            };

            var contentTypeInfo2 = new ContentTypeInfo(contentTypeId2, "ContentTypeNameKey2", "ContentTypeDescKey2", "GroupKey")
            {
                Fields = new List<IFieldInfo>()
                    {
                        testFieldInfo
                    }
            };

            var listInfo = new ListInfo(Url, "NameKey", "DescriptionKey")
            {
                ContentTypes = new List<ContentTypeInfo>()
                    {
                        contentTypeInfo1,
                        contentTypeInfo2
                    }
            };

            using (var testScope = SiteTestScope.BlankSite())
            {
                var rootWeb = testScope.SiteCollection.RootWeb;

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var listHelper = injectionScope.Resolve<IListHelper>();
                    
                    // Ensure a first time with ordering #1 > #2
                    var list = listHelper.EnsureList(rootWeb, listInfo);

                    // Change the ordering
                    listInfo.ContentTypes = new List<ContentTypeInfo>()
                        {
                            contentTypeInfo2,
                            contentTypeInfo1
                        };

                    // Act: Re-provision to update the ordering
                    list = listHelper.EnsureList(rootWeb, listInfo);
                    var listRefetched = rootWeb.Lists[list.ID];

                    // Assert
                    var listUniqueContentTypeOrder = list.RootFolder.UniqueContentTypeOrder;
                    Assert.IsTrue(listUniqueContentTypeOrder[0].Id.IsChildOf(new SPContentTypeId(contentTypeInfo2.ContentTypeId)));
                    Assert.IsTrue(listUniqueContentTypeOrder[1].Id.IsChildOf(new SPContentTypeId(contentTypeInfo1.ContentTypeId)));

                    var listUniqueContentTypeOrderRefetched = listRefetched.RootFolder.UniqueContentTypeOrder;
                    Assert.IsTrue(listUniqueContentTypeOrderRefetched[0].Id.IsChildOf(new SPContentTypeId(contentTypeInfo2.ContentTypeId)));
                    Assert.IsTrue(listUniqueContentTypeOrderRefetched[1].Id.IsChildOf(new SPContentTypeId(contentTypeInfo1.ContentTypeId)));
                }
            }
        }

        #endregion

        #region Content Type title, description and content group should be localizable

        //// TODO: add en/fr Resource tests here

        #endregion
    }
}
