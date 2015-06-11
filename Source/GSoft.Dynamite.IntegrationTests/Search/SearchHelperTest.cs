using System;
using System.Collections.Generic;
using Autofac;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Search.Enums;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSoft.Dynamite.IntegrationTests.Search
{
    /// <summary>
    /// Validates the entire stack of behavior behind <see cref="SearchHelper"/>.
    /// The GSoft.Dynamite.wsp package (GSoft.Dynamite.SP project) needs to be 
    /// deployed to the current server environment before running these tests.
    /// Redeploy the WSP package every time GSoft.Dynamite.dll changes.
    /// </summary>
    [TestClass]
    public class SearchHelperTest
    {
        #region EnsureResultSource should apply the right ranking model (if appropriate)

        /// <summary>
        /// Validates that EnsureResultSource applies the appropriate ranking model if a sorting by rank is specified
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureResultSource_WhenSortingByRank_ShouldApplySpecifiedRankingModel()
        {
            const string ResultSourceName = "Test Result Source";

            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                var resultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = "{?{searchTerms} -ContentClass=urn:content-class:SPSPeople}",
                    SortSettings = new Dictionary<string, SortDirection>()
                    {
                        { BuiltInManagedProperties.Rank.Name, SortDirection.Descending }
                    },
                    RankingModelId = BuiltInRankingModels.DefaultSearchModelId,
                    UpdateMode = ResultSourceUpdateBehavior.OverwriteResultSource
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);
                    var federationManager = new FederationManager(ssa);
                    var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, testScope.SiteCollection.RootWeb);

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);

                    // Assert
                    var source = federationManager.GetSourceByName(ResultSourceName, searchOwner);

                    Assert.IsNotNull(source);
                    Assert.AreEqual(ResultSourceName, source.Name);
                    Assert.AreEqual(BuiltInRankingModels.DefaultSearchModelId.ToString(), source.QueryTransform.OverrideProperties["RankingModelId"]);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureResultSource applies the "Default Search Model" if Ranking is used but no model is specified
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureResultSource_WhenSortingByRank_ShouldApplyDefaultRankingModelIfNothingSpecified()
        {
            const string ResultSourceName = "Test Result Source";

            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                var resultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = "{?{searchTerms} -ContentClass=urn:content-class:SPSPeople}",
                    SortSettings = new Dictionary<string, SortDirection>()
                    {
                        { BuiltInManagedProperties.Rank.Name, SortDirection.Descending }
                    },
                    UpdateMode = ResultSourceUpdateBehavior.OverwriteResultSource
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);
                    var federationManager = new FederationManager(ssa);
                    var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, testScope.SiteCollection.RootWeb);

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);

                    // Assert
                    var source = federationManager.GetSourceByName(ResultSourceName, searchOwner);

                    Assert.IsNotNull(source);
                    Assert.AreEqual(ResultSourceName, source.Name);
                    Assert.AreEqual(BuiltInRankingModels.DefaultSearchModelId.ToString(), source.QueryTransform.OverrideProperties["RankingModelId"]);
                }
            }
        }

        /// <summary>
        /// Validates that EnsureResultSource doesn't apply a ranking model if Rank is not in the specified SortSettings
        /// </summary>
        [TestMethod]
        [TestCategory(IntegrationTestCategories.Sanity)]
        public void EnsureResultSource_WhenNotSortingByRank_ShouldNotApplyARankingModel()
        {
            const string ResultSourceName = "Test Result Source";

            // Arrange
            using (var testScope = SiteTestScope.BlankSite())
            {
                var resultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = "{?{searchTerms} -ContentClass=urn:content-class:SPSPeople}",
                    RankingModelId = BuiltInRankingModels.DefaultSearchModelId,
                    UpdateMode = ResultSourceUpdateBehavior.OverwriteResultSource
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);
                    var federationManager = new FederationManager(ssa);
                    var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, testScope.SiteCollection.RootWeb);

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);

                    // Assert
                    var source = federationManager.GetSourceByName(ResultSourceName, searchOwner);
                    var sortListProperty = source.QueryTransform.OverrideProperties["SortList"] as SortCollection;

                    Assert.IsNotNull(source);
                    Assert.AreEqual(ResultSourceName, source.Name);
                    Assert.AreEqual(0, sortListProperty.Count);
                    Assert.IsFalse(source.QueryTransform.OverrideProperties.ContainsKey("RankingModelId"));
                }
            }
        }

        #endregion
    }
}
