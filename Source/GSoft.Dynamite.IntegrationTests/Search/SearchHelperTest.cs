using System;
using System.Collections.Generic;
using System.Globalization;
using Autofac;
using GSoft.Dynamite.Search;
using GSoft.Dynamite.Search.Enums;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagedPropertyInfo = GSoft.Dynamite.Search.ManagedPropertyInfo;

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
        /// Validates that EnsureResultSource throws an exception if a Ranking Model ID is specified but Rank is not found in the specified SortSettings
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
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

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);

                    // Assert

                    // Exception should have been thrown already
                    Assert.IsTrue(false);
                }
            }
        }

        #endregion

        #region EnsureResultSource should append and revert query changes

        /// <summary>
        /// Validates that the EnsureResultSource appends the query correctly when using "AppendToQuery" update mode.
        /// </summary>
        [TestMethod]
        public void EnsureResultSource_WhenAppendingQuery_ShouldAppendToEndOfExistingQuery()
        {
            const string ResultSourceName = "Test Result Source";
            const string Query = "{?{searchTerms} -ContentClass=urn:content-class:SPSPeople}";
            const string AppendedQuery = "{?{|owstaxidmetadataalltagsinfo:{User.SPSResponsibility}}}";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var resultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = Query,
                    UpdateMode = ResultSourceUpdateBehavior.OverwriteResultSource
                };

                var appendedResultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = AppendedQuery,
                    UpdateMode = ResultSourceUpdateBehavior.AppendToQuery
                };

                var expectedQuery = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Query, AppendedQuery);

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);
                    var federationManager = new FederationManager(ssa);
                    var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, testScope.SiteCollection.RootWeb);

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);
                    searchHelper.EnsureResultSource(testScope.SiteCollection, appendedResultSourceInfo);

                    // Assert
                    var source = federationManager.GetSourceByName(ResultSourceName, searchOwner);

                    Assert.IsNotNull(source);
                    Assert.IsNotNull(source.QueryTransform);
                    Assert.AreEqual(ResultSourceName, source.Name);
                    Assert.AreEqual(expectedQuery, source.QueryTransform.QueryTemplate);
                }
            }
        }

        /// <summary>
        /// Validates that the EnsureResultSource appends the query correctly when using "AppendToQuery" update mode
        /// and then reverts the query to the original state using "RevertQuery" update method.
        /// </summary>
        [TestMethod]
        public void EnsureResultSource_WhenRevertingAppendedQuery_ShouldRevertToPreviousQuery()
        {
            const string ResultSourceName = "Test Result Source";
            const string Query = "{?{searchTerms} -ContentClass=urn:content-class:SPSPeople}";
            const string AppendedQuery = "{?{|owstaxidmetadataalltagsinfo:{User.SPSResponsibility}}}";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var resultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = Query,
                    UpdateMode = ResultSourceUpdateBehavior.OverwriteResultSource
                };

                var appendedResultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = AppendedQuery,
                    UpdateMode = ResultSourceUpdateBehavior.AppendToQuery
                };

                var revertedResultSourceInfo = new ResultSourceInfo()
                {
                    Name = ResultSourceName,
                    Level = SearchObjectLevel.SPSite,
                    Query = AppendedQuery,
                    UpdateMode = ResultSourceUpdateBehavior.RevertQuery
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);
                    var federationManager = new FederationManager(ssa);
                    var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, testScope.SiteCollection.RootWeb);

                    // Act
                    searchHelper.EnsureResultSource(testScope.SiteCollection, resultSourceInfo);
                    searchHelper.EnsureResultSource(testScope.SiteCollection, appendedResultSourceInfo);
                    searchHelper.EnsureResultSource(testScope.SiteCollection, revertedResultSourceInfo);

                    // Assert
                    var source = federationManager.GetSourceByName(ResultSourceName, searchOwner);

                    Assert.IsNotNull(source);
                    Assert.IsNotNull(source.QueryTransform);
                    Assert.AreEqual(ResultSourceName, source.Name);
                    Assert.AreEqual(Query, source.QueryTransform.QueryTemplate);
                }
            }
        }

        #endregion

        #region EnsureManagedProperty

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' and the property doesn't exist,
        /// it creates a new managed property with the correct condfiguration and crawled property mappings.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenOverwriteIfAlreadyExists_ShouldCreateTheManagedProperty()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(true, actualManagedProperty.Sortable);
                        Assert.AreEqual(true, actualManagedProperty.Refinable);
                        Assert.AreEqual(true, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(1)[0].Name == "ows_Title");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Title");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it overwrites the configuration and crawled property mappings.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenOverwriteIfAlreadyExists_ShouldOverwriteExistingManagedProperty()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                var overwrittenManagedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = false,
                    Refinable = false,
                    Queryable = false,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Description", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, overwrittenManagedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(false, actualManagedProperty.Sortable);
                        Assert.AreEqual(false, actualManagedProperty.Refinable);
                        Assert.AreEqual(false, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(1)[0].Name == "ows_Description");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Description");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it appends the added crawled properties mappings (if different) and keeps the configuration untouched.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenAppendingCrawledProperties_ShouldAppendCrawledPropertiesAndKeepConfiguration()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                var appendedManagedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = false,
                    Refinable = false,
                    Queryable = false,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Description", 2 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.AppendCrawledProperties
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, appendedManagedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(true, actualManagedProperty.Sortable);
                        Assert.AreEqual(true, actualManagedProperty.Refinable);
                        Assert.AreEqual(true, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[0].Name == "ows_Title");
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[1].Name == "ows_Description");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Title");
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[1].CrawledPropertyName == "ows_Description");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it overwrites the crawled properties mappings (if different) and keeps the configuration untouched.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenOverwritingCrawledProperties_ShouldOverwriteCrawledPropertiesAndKeepConfiguration()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                var overwrittenManagedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = false,
                    Refinable = false,
                    Queryable = false,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Description", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteCrawledProperties
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, overwrittenManagedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(true, actualManagedProperty.Sortable);
                        Assert.AreEqual(true, actualManagedProperty.Refinable);
                        Assert.AreEqual(true, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2).Count == 1);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[0].Name == "ows_Description");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner).Count == 1);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Description");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it changes the configuration while keeping the crawled property mappings untouched.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenUpdatingConfiguration_ShouldChangeConfigurationAndKeepCrawledPropertyMappings()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                var updatedManagedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = false,
                    Refinable = false,
                    Queryable = false,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Description", 2 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.UpdateConfiguration
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, updatedManagedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(false, actualManagedProperty.Sortable);
                        Assert.AreEqual(false, actualManagedProperty.Refinable);
                        Assert.AreEqual(false, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2).Count == 1);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[0].Name == "ows_Title");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(false, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner).Count == 1);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Title");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it simply fetches the managed property and leaves the configuration and crawled property mappings untouched.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenNoChangesWanted_ShouldOnlyFetchManagedProperty()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists
                };

                var noChangesManagedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = false,
                    Refinable = false,
                    Queryable = false,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Description", 2 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.NoChangesIfAlreadyExists
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, noChangesManagedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(true, actualManagedProperty.Sortable);
                        Assert.AreEqual(true, actualManagedProperty.Refinable);
                        Assert.AreEqual(true, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2).Count == 1);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[0].Name == "ows_Title");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner).Count == 1);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Title");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Validates that when calling 'EnsureManagedProperty' on an existing property,
        /// it creates the managed property with the 'NoChangesIfAlreadyExists' update mode.
        /// </summary>
        [TestMethod]
        public void EnsureManagedProperty_WhenNoChangesWanted_ShouldCreateManagedPropertyIfDoesntExist()
        {
            const string ManagedPropertyName = "TestManagedProperty";

            using (var testScope = SiteTestScope.BlankSite())
            {
                // Arrange
                var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, testScope.SiteCollection.RootWeb);
                var managedPropertyInfo = new ManagedPropertyInfo(ManagedPropertyName, ManagedDataType.Text)
                {
                    Sortable = true,
                    Refinable = true,
                    Queryable = true,
                    CrawledProperties = new Dictionary<string, int>()
                    {
                        { "ows_Title", 1 }
                    },
                    UpdateBehavior = ManagedPropertyUpdateBehavior.NoChangesIfAlreadyExists
                };

                using (var injectionScope = IntegrationTestServiceLocator.BeginLifetimeScope())
                {
                    var searchHelper = injectionScope.Resolve<ISearchHelper>();
                    var ssa = searchHelper.GetDefaultSearchServiceApplication(testScope.SiteCollection);

                    try
                    {
                        // Act
                        var actualManagedProperty = searchHelper.EnsureManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                        var refetchedActualManagedProperty = ssa.GetManagedProperty(ManagedPropertyName, owner);

                        // Assert
                        Assert.IsNotNull(actualManagedProperty);
                        Assert.AreEqual(true, actualManagedProperty.Sortable);
                        Assert.AreEqual(true, actualManagedProperty.Refinable);
                        Assert.AreEqual(true, actualManagedProperty.Queryable);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2).Count == 1);
                        Assert.IsTrue(actualManagedProperty.GetMappedCrawledProperties(2)[0].Name == "ows_Title");

                        Assert.IsNotNull(refetchedActualManagedProperty);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Sortable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Refinable);
                        Assert.AreEqual(true, refetchedActualManagedProperty.Queryable);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner).Count == 1);
                        Assert.IsTrue(ssa.GetManagedPropertyMappings(refetchedActualManagedProperty, owner)[0].CrawledPropertyName == "ows_Title");
                    }
                    finally
                    {
                        // Clean up
                        searchHelper.DeleteManagedProperty(testScope.SiteCollection, managedPropertyInfo);
                    }
                }
            }
        }

        #endregion
    }
}
