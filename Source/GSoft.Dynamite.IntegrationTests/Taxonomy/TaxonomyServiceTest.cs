using System;
using System.Globalization;
using System.Linq;
using Autofac;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Taxonomy
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="TaxonomyService"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class TaxonomyServiceTest
    {
        #region Interacting with terms used as simple link navigation nodes

        private const string CustomPropertyKey = "_Sys_Nav_SimpleLinkUrl";

        /// <summary>
        /// Validates that when you want to get a term set with some terms, and those terms do not have child terms,
        /// you should get a list of those first level terms of type SimpleLinkTermInfo.
        /// </summary>
        [TestMethod]
        public void GetTermsAsSimpleLinkNavNodeForTermSet_WhenNoChildTerms_ShouldReturnAllFirstLevelTerms()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var testTermSetInfo = new TermSetInfo(Guid.NewGuid(), "Test Term Set");
                var levelOneTermA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term A", testTermSetInfo, "URL-A");
                var levelOneTermB = new SimpleLinkTermInfo(Guid.NewGuid(), "Term B", testTermSetInfo, "URL-B");
                var levelOneTermC = new SimpleLinkTermInfo(Guid.NewGuid(), "Term C", testTermSetInfo, "URL-C");

                var expecteNumberOfTerms = 3;

                var session = new TaxonomySession(testScope.SiteCollection);
                var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                var defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                var newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSetInfo.Label, testTermSetInfo.Id);
                
                var createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                createdTermA.SetLocalCustomProperty(CustomPropertyKey, levelOneTermA.SimpleLinkTarget);

                var createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                createdTermB.SetLocalCustomProperty(CustomPropertyKey, levelOneTermB.SimpleLinkTarget);

                var createdTermC = newTermSet.CreateTerm(levelOneTermC.Label, Language.English.Culture.LCID, levelOneTermC.Id);
                createdTermC.SetLocalCustomProperty(CustomPropertyKey, levelOneTermC.SimpleLinkTarget);

                defaultSiteCollectionTermStore.CommitAll();

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var taxonomyService = injectionScope.Resolve<ITaxonomyService>();

                    // Act
                    var retrievedTerms = taxonomyService.GetTermsAsSimpleLinkNavNodeForTermSet(testScope.SiteCollection, defaultSiteCollectionGroup.Name, newTermSet.Name);

                    // Assert
                    Assert.IsNotNull(retrievedTerms);
                    Assert.AreEqual(retrievedTerms.Count, expecteNumberOfTerms);

                    Assert.AreEqual(retrievedTerms[0].SimpleLinkTarget, levelOneTermA.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[0].Label, levelOneTermA.Label);
                    Assert.AreEqual(retrievedTerms[0].ChildTerms.Count(), 0);

                    Assert.AreEqual(retrievedTerms[1].SimpleLinkTarget, levelOneTermB.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[1].Label, levelOneTermB.Label);
                    Assert.AreEqual(retrievedTerms[1].ChildTerms.Count(), 0);

                    Assert.AreEqual(retrievedTerms[2].SimpleLinkTarget, levelOneTermC.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[2].Label, levelOneTermC.Label);
                    Assert.AreEqual(retrievedTerms[2].ChildTerms.Count(), 0);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that when you want to get a term set with some terms, and those terms have child terms,
        /// you should get a list of all level one terms, and each one should contain its child terms.
        /// </summary>
        [TestMethod]
        public void GetTermsAsSimpleLinkNavNodeForTermSet_WithChildTerms_ShouldReturnAllFirstLevelTermsWithChild()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var testTermSetInfo = new TermSetInfo(Guid.NewGuid(), "Test Term Set");
                var levelOneTermA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term A", testTermSetInfo, "URL-A");
                var levelOneTermB = new SimpleLinkTermInfo(Guid.NewGuid(), "Term B", testTermSetInfo, "URL-B");
                var levelTwoTermAA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term AA", testTermSetInfo, "URL-AA");
                var levelTwoTermAB = new SimpleLinkTermInfo(Guid.NewGuid(), "Term AB", testTermSetInfo, "URL-AB");
                var levelTwoTermBA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term BA", testTermSetInfo, "URL-BA");

                var expecteNumberOfLevelOneTerms = 2;
                var expectedTotalNumberOfTerms = 5;

                var session = new TaxonomySession(testScope.SiteCollection);
                var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                var defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                var newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSetInfo.Label, testTermSetInfo.Id);

                var createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                createdTermA.SetLocalCustomProperty(CustomPropertyKey, levelOneTermA.SimpleLinkTarget);
                var createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                createdTermAA.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermAA.SimpleLinkTarget);
                var createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                createdTermAB.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermAB.SimpleLinkTarget);

                var createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                createdTermB.SetLocalCustomProperty(CustomPropertyKey, levelOneTermB.SimpleLinkTarget);
                var createdTermBA = createdTermB.CreateTerm(levelTwoTermBA.Label, Language.English.Culture.LCID, levelTwoTermBA.Id);
                createdTermBA.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermBA.SimpleLinkTarget);

                defaultSiteCollectionTermStore.CommitAll();

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var taxonomyService = injectionScope.Resolve<ITaxonomyService>();

                    // Act
                    var retrievedTerms = taxonomyService.GetTermsAsSimpleLinkNavNodeForTermSet(testScope.SiteCollection, defaultSiteCollectionGroup.Name, newTermSet.Name);

                    // Assert
                    Assert.IsNotNull(retrievedTerms);
                    Assert.AreEqual(retrievedTerms.Count, expecteNumberOfLevelOneTerms);

                    var actualTotalNumberOfTermsRetrieved = retrievedTerms.Count + retrievedTerms[0].ChildTerms.Count() + retrievedTerms[1].ChildTerms.Count();
                    Assert.AreEqual(actualTotalNumberOfTermsRetrieved, expectedTotalNumberOfTerms);

                    Assert.AreEqual(retrievedTerms[0].SimpleLinkTarget, levelOneTermA.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[0].Label, levelOneTermA.Label);
                    Assert.AreEqual(retrievedTerms[0].ChildTerms.Count(), 2);

                    Assert.AreEqual(retrievedTerms[1].SimpleLinkTarget, levelOneTermB.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[1].Label, levelOneTermB.Label);
                    Assert.AreEqual(retrievedTerms[1].ChildTerms.Count(), 1);

                    var actualLevelTwoAATerm = retrievedTerms[0].ChildTerms.ElementAt(0);
                    var actualLevelTwoABTerm = retrievedTerms[0].ChildTerms.ElementAt(1);
                    var actualLevelTwoBATerm = retrievedTerms[1].ChildTerms.ElementAt(0);

                    Assert.AreEqual(actualLevelTwoAATerm.SimpleLinkTarget, levelTwoTermAA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoAATerm.Label, levelTwoTermAA.Label);
                    Assert.AreEqual(actualLevelTwoAATerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoABTerm.SimpleLinkTarget, levelTwoTermAB.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoABTerm.Label, levelTwoTermAB.Label);
                    Assert.AreEqual(actualLevelTwoABTerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoBATerm.SimpleLinkTarget, levelTwoTermBA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoBATerm.Label, levelTwoTermBA.Label);
                    Assert.AreEqual(actualLevelTwoBATerm.ChildTerms.Count(), 0);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that when you want to get a term set with some terms, some of them without a specified Simple Link URL,
        /// and those terms have child terms, you should get a list of all level one terms, and each one should contain its child terms.
        /// </summary>
        [TestMethod]
        public void GetTermsAsSimpleLinkNavNodeForTermSet_WithChildTermsAndSomeWithoutUrl_ShouldReturnAllFirstLevelTermsWithChild()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var testTermSetInfo = new TermSetInfo(Guid.NewGuid(), "Test Term Set");
                var levelOneTermA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term A", testTermSetInfo, "URL-A");
                var levelOneTermB = new SimpleLinkTermInfo(Guid.NewGuid(), "Term B", testTermSetInfo);
                var levelTwoTermAA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term AA", testTermSetInfo);
                var levelTwoTermAB = new SimpleLinkTermInfo(Guid.NewGuid(), "Term AB", testTermSetInfo, "URL-AB");
                var levelTwoTermBA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term BA", testTermSetInfo, "URL-BA");

                var expecteNumberOfLevelOneTerms = 2;
                var expectedTotalNumberOfTerms = 5;

                var session = new TaxonomySession(testScope.SiteCollection);
                var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                var defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                var newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSetInfo.Label, testTermSetInfo.Id);

                var createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                createdTermA.SetLocalCustomProperty(CustomPropertyKey, levelOneTermA.SimpleLinkTarget);
                var createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                var createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                createdTermAB.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermAB.SimpleLinkTarget);

                var createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                var createdTermBA = createdTermB.CreateTerm(levelTwoTermBA.Label, Language.English.Culture.LCID, levelTwoTermBA.Id);
                createdTermBA.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermBA.SimpleLinkTarget);

                defaultSiteCollectionTermStore.CommitAll();

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var taxonomyService = injectionScope.Resolve<ITaxonomyService>();

                    // Act
                    var retrievedTerms = taxonomyService.GetTermsAsSimpleLinkNavNodeForTermSet(testScope.SiteCollection, defaultSiteCollectionGroup.Name, newTermSet.Name);

                    // Assert
                    Assert.IsNotNull(retrievedTerms);
                    Assert.AreEqual(retrievedTerms.Count, expecteNumberOfLevelOneTerms);

                    var actualTotalNumberOfTermsRetrieved = retrievedTerms.Count + retrievedTerms[0].ChildTerms.Count() + retrievedTerms[1].ChildTerms.Count();
                    Assert.AreEqual(actualTotalNumberOfTermsRetrieved, expectedTotalNumberOfTerms);

                    Assert.AreEqual(retrievedTerms[0].SimpleLinkTarget, levelOneTermA.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[0].Label, levelOneTermA.Label);
                    Assert.AreEqual(retrievedTerms[0].ChildTerms.Count(), 2);

                    Assert.AreEqual(retrievedTerms[1].SimpleLinkTarget, levelOneTermB.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[1].Label, levelOneTermB.Label);
                    Assert.AreEqual(retrievedTerms[1].ChildTerms.Count(), 1);

                    var actualLevelTwoAATerm = retrievedTerms[0].ChildTerms.ElementAt(0);
                    var actualLevelTwoABTerm = retrievedTerms[0].ChildTerms.ElementAt(1);
                    var actualLevelTwoBATerm = retrievedTerms[1].ChildTerms.ElementAt(0);

                    Assert.AreEqual(actualLevelTwoAATerm.SimpleLinkTarget, levelTwoTermAA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoAATerm.Label, levelTwoTermAA.Label);
                    Assert.AreEqual(actualLevelTwoAATerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoABTerm.SimpleLinkTarget, levelTwoTermAB.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoABTerm.Label, levelTwoTermAB.Label);
                    Assert.AreEqual(actualLevelTwoABTerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoBATerm.SimpleLinkTarget, levelTwoTermBA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoBATerm.Label, levelTwoTermBA.Label);
                    Assert.AreEqual(actualLevelTwoBATerm.ChildTerms.Count(), 0);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        /// <summary>
        /// Validates that the custom sort order is taken into account when retrieving terms.
        /// </summary>
        [TestMethod]
        public void GetTermsAsSimpleLinkNavNodeForTermSet_WithChildTermsAndCustomSortOrder_ShouldKeepTheTermsOrder()
        {
            using (var testScope = SiteTestScope.BlankSite())
            {
                var guidTermA = Guid.NewGuid();
                var guidTermB = Guid.NewGuid();
                var guidTermAA = Guid.NewGuid();
                var guidTermAB = Guid.NewGuid();

                // Arrange
                var testTermSetInfo = new TermSetInfo(Guid.NewGuid(), "Test Term Set");
                var levelOneTermA = new SimpleLinkTermInfo(guidTermA, "Term A", testTermSetInfo, "URL-A");
                var levelOneTermB = new SimpleLinkTermInfo(guidTermB, "Term B", testTermSetInfo);
                var levelTwoTermAA = new SimpleLinkTermInfo(guidTermAA, "Term AA", testTermSetInfo);
                var levelTwoTermAB = new SimpleLinkTermInfo(guidTermAB, "Term AB", testTermSetInfo, "URL-AB");
                var levelTwoTermBA = new SimpleLinkTermInfo(Guid.NewGuid(), "Term BA", testTermSetInfo, "URL-BA");

                var expecteNumberOfLevelOneTerms = 2;
                var expectedTotalNumberOfTerms = 5;

                var session = new TaxonomySession(testScope.SiteCollection);
                var defaultSiteCollectionTermStore = session.DefaultSiteCollectionTermStore;
                var defaultSiteCollectionGroup = defaultSiteCollectionTermStore.GetSiteCollectionGroup(testScope.SiteCollection);
                var newTermSet = defaultSiteCollectionGroup.CreateTermSet(testTermSetInfo.Label, testTermSetInfo.Id);

                var createdTermA = newTermSet.CreateTerm(levelOneTermA.Label, Language.English.Culture.LCID, levelOneTermA.Id);
                createdTermA.SetLocalCustomProperty(CustomPropertyKey, levelOneTermA.SimpleLinkTarget);

                var createdTermAA = createdTermA.CreateTerm(levelTwoTermAA.Label, Language.English.Culture.LCID, levelTwoTermAA.Id);
                var createdTermAB = createdTermA.CreateTerm(levelTwoTermAB.Label, Language.English.Culture.LCID, levelTwoTermAB.Id);
                createdTermAB.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermAB.SimpleLinkTarget);

                var createdTermB = newTermSet.CreateTerm(levelOneTermB.Label, Language.English.Culture.LCID, levelOneTermB.Id);
                var createdTermBA = createdTermB.CreateTerm(levelTwoTermBA.Label, Language.English.Culture.LCID, levelTwoTermBA.Id);
                createdTermBA.SetLocalCustomProperty(CustomPropertyKey, levelTwoTermBA.SimpleLinkTarget);

                // Create a custom sort order where term B is first, term A is second
                newTermSet.CustomSortOrder = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", guidTermB, guidTermA);

                // Create a custom sort order where term AB is first, term AA is second
                createdTermA.CustomSortOrder = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", guidTermAB, guidTermAA);

                defaultSiteCollectionTermStore.CommitAll();

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var taxonomyService = injectionScope.Resolve<ITaxonomyService>();

                    // Act
                    var retrievedTerms = taxonomyService.GetTermsAsSimpleLinkNavNodeForTermSet(testScope.SiteCollection, defaultSiteCollectionGroup.Name, newTermSet.Name);

                    // Assert
                    Assert.IsNotNull(retrievedTerms);
                    Assert.AreEqual(retrievedTerms.Count, expecteNumberOfLevelOneTerms);

                    var actualTotalNumberOfTermsRetrieved = retrievedTerms.Count + retrievedTerms[0].ChildTerms.Count() + retrievedTerms[1].ChildTerms.Count();
                    Assert.AreEqual(actualTotalNumberOfTermsRetrieved, expectedTotalNumberOfTerms);

                    Assert.AreEqual(retrievedTerms[0].SimpleLinkTarget, levelOneTermB.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[0].Label, levelOneTermB.Label);
                    Assert.AreEqual(retrievedTerms[0].ChildTerms.Count(), 1);

                    Assert.AreEqual(retrievedTerms[1].SimpleLinkTarget, levelOneTermA.SimpleLinkTarget);
                    Assert.AreEqual(retrievedTerms[1].Label, levelOneTermA.Label);
                    Assert.AreEqual(retrievedTerms[1].ChildTerms.Count(), 2);

                    var actualLevelTwoAATerm = retrievedTerms[1].ChildTerms.ElementAt(1);
                    var actualLevelTwoABTerm = retrievedTerms[1].ChildTerms.ElementAt(0);
                    var actualLevelTwoBATerm = retrievedTerms[0].ChildTerms.ElementAt(0);

                    Assert.AreEqual(actualLevelTwoAATerm.SimpleLinkTarget, levelTwoTermAA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoAATerm.Label, levelTwoTermAA.Label);
                    Assert.AreEqual(actualLevelTwoAATerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoABTerm.SimpleLinkTarget, levelTwoTermAB.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoABTerm.Label, levelTwoTermAB.Label);
                    Assert.AreEqual(actualLevelTwoABTerm.ChildTerms.Count(), 0);

                    Assert.AreEqual(actualLevelTwoBATerm.SimpleLinkTarget, levelTwoTermBA.SimpleLinkTarget);
                    Assert.AreEqual(actualLevelTwoBATerm.Label, levelTwoTermBA.Label);
                    Assert.AreEqual(actualLevelTwoBATerm.ChildTerms.Count(), 0);
                }

                // Cleanup term set so that we don't pollute the metadata store
                newTermSet.Delete();
                defaultSiteCollectionTermStore.CommitAll();
            }
        }

        #endregion
    }
}
