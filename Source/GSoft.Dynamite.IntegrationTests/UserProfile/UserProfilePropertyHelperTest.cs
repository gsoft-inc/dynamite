using System;
using Autofac;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.UserProfile;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.UserProfile
{
    /// <summary>
    /// Validates the behavior of the default-configured implementation 
    /// of <see cref="IUserProfilePropertyHelper"/>, the user profile property helper interface.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class UserProfilePropertyHelperTest
    {
        private const string ProfilePropertyName = "testProfileProperty";
        private const string TermSetName = "Test Term Set";

        /// <summary>
        /// Cleans up the test data.
        /// </summary>
        public void TestCleanup()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var site = testScope.SiteCollection;

                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                ProfilePropertyName,
                "Test Profile Property",
                PropertyDataType.StringSingleValue);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    // Try removing the test profile property
                    try
                    {
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        userProfileHelper.RemoveProfileProperty(site, userProfilePropertyInfo);
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to Ensure a user profile property creates it
        /// if it doesn't already exist.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenCreatingProperty_GivenNewUserProfilePropertyInfo_ThenCreatesProperty()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;

                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                    ProfilePropertyName,
                    "Test Profile Property",
                    PropertyDataType.StringSingleValue);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        var userProfileProperty = userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Assert
                        Assert.IsNotNull(userProfileProperty);
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to Ensure a user profile property updates
        /// the display name if it already exists.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenUpdatingDisplayName_GivenUpdatedUserProfilePropertyInfo_ThenUpdatesProperty()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;
                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                    ProfilePropertyName,
                    "Test Profile Property",
                    PropertyDataType.StringSingleValue);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);
                        userProfilePropertyInfo.DisplayName = "Test Profile Property Updated";
                        var userProfileProperty = userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Assert
                        Assert.AreEqual(userProfileProperty.DisplayName, "Test Profile Property Updated");
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property updates
        /// the visilibility properties if it already exists.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenUpdatingVisibility_GivenUpdatedUserProfilePropertyInfo_ThenUpdatesProperty()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;
                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                ProfilePropertyName,
                "Test Profile Property",
                PropertyDataType.StringSingleValue);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);
                        userProfilePropertyInfo.IsVisibleOnEditor = true;
                        userProfilePropertyInfo.IsVisibleOnViewer = true;
                        userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Assert
                        var profileTypeManager = userProfileHelper.GetProfileTypePropertyManager(site);
                        var profileTypeProperty = profileTypeManager.GetPropertyByName(userProfilePropertyInfo.Name);
                        Assert.AreEqual(profileTypeProperty.IsVisibleOnEditor, userProfilePropertyInfo.IsVisibleOnEditor);
                        Assert.AreEqual(profileTypeProperty.IsVisibleOnViewer, userProfilePropertyInfo.IsVisibleOnViewer);
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property updates
        /// the security properties if it already exists.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenUpdatingSecurity_GivenUpdatedUserProfilePropertyInfo_ThenUpdatesProperty()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;
                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                ProfilePropertyName,
                "Test Profile Property",
                PropertyDataType.StringSingleValue)
                {
                    IsUserEditable = true
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Make sure the default privacy value is set to private
                        var profileSubtypeManager = userProfileHelper.GetProfileSubtypePropertyManager(site);
                        var profileSubtypeProperty = profileSubtypeManager.GetPropertyByName(userProfilePropertyInfo.Name);

                        Assert.AreEqual(Privacy.Private, profileSubtypeProperty.DefaultPrivacy);

                        userProfilePropertyInfo.IsUserEditable = false;
                        userProfilePropertyInfo.DefaultPrivacy = Privacy.Public;
                        userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        profileSubtypeManager = userProfileHelper.GetProfileSubtypePropertyManager(site);
                        profileSubtypeProperty = profileSubtypeManager.GetPropertyByName(userProfilePropertyInfo.Name);

                        // Assert
                        Assert.AreEqual(profileSubtypeProperty.IsUserEditable, userProfilePropertyInfo.IsUserEditable);
                        Assert.AreEqual(profileSubtypeProperty.DefaultPrivacy, userProfilePropertyInfo.DefaultPrivacy);
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property assigns
        /// the term set property with a custom term set.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenMappedToTermSet_GivenUserProfilePropertyInfo_ThenMapsPropertyToTermSet()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;
                var termSetInfo = new TermSetInfo(Guid.NewGuid(), TermSetName);
                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                    ProfilePropertyName,
                    "Test Profile Property",
                    PropertyDataType.StringSingleValue)
                {
                    TermSetInfo = termSetInfo
                };

                // Create term set
                var session = new TaxonomySession(site);
                var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                var defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(site);
                defaultSiteCollectionGroup.CreateTermSet(termSetInfo.Label, termSetInfo.Id);
                defaultSiteCollectionTermStore.CommitAll();

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        var userProfileProperty = userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Assert
                        Assert.AreEqual(userProfileProperty.TermSet.Id, termSetInfo.Id);
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }

        /// <summary>
        /// Validates that using the IUserProfileHelper to ensure a user profile property
        /// sets up the localized display name and description properly.
        /// </summary>
        [TestMethod]
        public void EnsureProfileProperty_WhenLocalizing_GivenUserProfilePropertyInfo_ThenAddsLocalizedValues()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var site = testScope.SiteCollection;
                var userProfilePropertyInfo = new UserProfilePropertyInfo(
                    ProfilePropertyName,
                    "Test Profile Property",
                    PropertyDataType.StringSingleValue)
                {
                    Description = "Test property description"
                };

                userProfilePropertyInfo.DisplayNameLocalized.Add(1036, "Propriété de test");
                userProfilePropertyInfo.DescriptionLocalized.Add(1036, "Description propriété de test");

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope(site))
                {
                    try
                    {
                        // Act
                        var userProfileHelper = injectionScope.Resolve<IUserProfilePropertyHelper>();
                        var userProfileProperty = userProfileHelper.EnsureProfileProperty(site, userProfilePropertyInfo);

                        // Assert
                        // 2 values: Default (english and french/1036)
                        Assert.IsTrue(userProfileProperty.DisplayNameLocalized.Count == 2);
                        Assert.IsTrue(userProfileProperty.DescriptionLocalized.Count == 2);
                    }
                    finally
                    {
                        this.TestCleanup();
                    }
                }
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
