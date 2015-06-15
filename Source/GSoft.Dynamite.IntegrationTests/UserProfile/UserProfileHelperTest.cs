using System;
using Autofac;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.UserProfile;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.UserProfile
{
    /// <summary>
    /// Validates the behavior of the default-configured implementation 
    /// of <see cref="IUserProfileHelper"/>, the user profile porperty helper interface.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class UserProfileHelperTest
    {
        private static SPSite CentralAdminSite
        {
            get
            {
                return SPAdministrationWebApplication.Local.Sites[0];
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to Ensure a user profile property creates it
        /// if it doesn't already exist.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenCreatingProperty_GivenNewUserProfilePropertyInfo_ThenCreatesProperty()
        {
            // Arrange
            var userProfilePropertyInfo = new UserProfilePropertyInfo(
                "testProfileProperty",
                "Test Profile Property",
                PropertyDataType.StringSingleValue);

            using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(CentralAdminSite))
            {
                // Act
                var userProfileHelper = injectionScope.Resolve<IUserProfileHelper>();
                var userProfileProperty = userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);

                // Assert
                Assert.IsNotNull(userProfileProperty);

                // Clean up
                var isRemoved = userProfileHelper.RemoveProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                Assert.IsTrue(isRemoved);
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to Ensure a user profile property updates
        /// the display name if it already exists.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenUpdatingDisplayName_GivenUpdatedUserProfilePropertyInfo_ThenUpdatesProperty()
        {
            // Arrange
            var userProfilePropertyInfo = new UserProfilePropertyInfo(
                "testProfileProperty",
                "Test Profile Property",
                PropertyDataType.StringSingleValue);

            using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(CentralAdminSite))
            {
                // Act
                var userProfileHelper = injectionScope.Resolve<IUserProfileHelper>();
                userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                userProfilePropertyInfo.DisplayName = "Test Profile Property Updated";
                var userProfileProperty = userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);

                // Assert
                Assert.AreEqual(userProfileProperty.DisplayName, "Test Profile Property Updated");

                // Clean up
                var isRemoved = userProfileHelper.RemoveProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                Assert.IsTrue(isRemoved);
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property updates
        /// the visilibility properties if it already exists.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenUpdatingVisibility_GivenUpdatedUserProfilePropertyInfo_ThenUpdatesProperty()
        {
            // Arrange
            var userProfilePropertyInfo = new UserProfilePropertyInfo(
                "testProfileProperty",
                "Test Profile Property",
                PropertyDataType.StringSingleValue);

            using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(CentralAdminSite))
            {
                // Act
                var userProfileHelper = injectionScope.Resolve<IUserProfileHelper>();
                userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                userProfilePropertyInfo.IsVisibleOnEditor = true;
                userProfilePropertyInfo.IsVisibleOnViewer = true;
                userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);

                // Assert
                var profileTypeManager = userProfileHelper.GetProfileTypePropertyManager(CentralAdminSite);
                var profileTypeProperty = profileTypeManager.GetPropertyByName(userProfilePropertyInfo.Name);
                Assert.AreEqual(profileTypeProperty.IsVisibleOnEditor, userProfilePropertyInfo.IsVisibleOnEditor);
                Assert.AreEqual(profileTypeProperty.IsVisibleOnViewer, userProfilePropertyInfo.IsVisibleOnViewer);

                // Clean up
                var isRemoved = userProfileHelper.RemoveProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                Assert.IsTrue(isRemoved);
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property assigns
        /// the term set property with a custom term set.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenMappedToTermSet_GivenUserProfilePropertyInfo_ThenMapsPropertyToTermSet()
        {
            // Arrange
            var termGroupInfo = new TermGroupInfo(Guid.NewGuid(), "Test Group");
            var termSetInfo = new TermSetInfo(Guid.NewGuid(), "Test Term Set", termGroupInfo);
            var userProfilePropertyInfo = new UserProfilePropertyInfo(
                "testProfileProperty",
                "Test Profile Property",
                PropertyDataType.StringSingleValue)
                {
                    TermSetInfo = termSetInfo
                };

            // Create term set
            var session = new TaxonomySession(CentralAdminSite);
            var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
            var group = defaultSiteCollectionTermStore.CreateGroup(termGroupInfo.Name, termGroupInfo.Id);
            var termSet = group.CreateTermSet(termSetInfo.Label, termSetInfo.Id);
            defaultSiteCollectionTermStore.CommitAll();

            using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(CentralAdminSite))
            {
                // Act
                var userProfileHelper = injectionScope.Resolve<IUserProfileHelper>();
                var userProfileProperty = userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);

                // Assert
                Assert.AreEqual(userProfileProperty.TermSet.Id, termSetInfo.Id);

                // Clean up
                var isPropertyRemoved = userProfileHelper.RemoveProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                termSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
                Assert.IsTrue(isPropertyRemoved);
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property
        /// sets up the localized display name and description properly.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenLocalizing_GivenUserProfilePropertyInfo_ThenAddsLocalizedValues()
        {
            // Arrange
            var userProfilePropertyInfo = new UserProfilePropertyInfo(
                "testProfileProperty",
                "Test Profile Property",
                PropertyDataType.StringSingleValue)
                {
                    Description = "Test property description"
                };

            userProfilePropertyInfo.DisplayNameLocalized.Add(1036, "Propriété de test");
            userProfilePropertyInfo.DescriptionLocalized.Add(1036, "Description propriété de test");

            using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(CentralAdminSite))
            {
                // Act
                var userProfileHelper = injectionScope.Resolve<IUserProfileHelper>();
                var userProfileProperty = userProfileHelper.EnsureProfileProperty(CentralAdminSite, userProfilePropertyInfo);

                // Assert
                // 2 values: Default (english and french/1036)
                Assert.IsTrue(userProfileProperty.DisplayNameLocalized.Count == 2);
                Assert.IsTrue(userProfileProperty.DescriptionLocalized.Count == 2);

                // Clean up
                var isRemoved = userProfileHelper.RemoveProfileProperty(CentralAdminSite, userProfilePropertyInfo);
                Assert.IsTrue(isRemoved);
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
