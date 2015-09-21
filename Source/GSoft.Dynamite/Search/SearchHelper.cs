using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search.Enums;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Taxonomy;
using Source = Microsoft.Office.Server.Search.Administration.Query.Source;
using SPManagedPropertyInfo = Microsoft.Office.Server.Search.Administration.ManagedPropertyInfo;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Search service utilities
    /// </summary>
    public class SearchHelper : ISearchHelper
    {
        private readonly ILogger logger;
        private readonly ITaxonomyService taxonomyService;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="taxonomyService">The taxonomy service instance</param>
        public SearchHelper(ILogger logger, ITaxonomyService taxonomyService)
        {
            this.logger = logger;
            this.taxonomyService = taxonomyService;
        }

        /// <summary>
        /// Gets the default search service application from a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The search service application.</returns>
        public SearchServiceApplication GetDefaultSearchServiceApplication(SPSite site)
        {
            var context = SPServiceContext.GetContext(site);

            // Get the search service application proxy
            var searchProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy)) as SearchServiceApplicationProxy;

            // Get the search service application info object so we can find the Id of our Search Service App
            if (searchProxy != null)
            {
                var applicationInfo = searchProxy.GetSearchServiceApplicationInfo();

                // Get the application itself
                return SearchService.Service.SearchApplications.GetValue<SearchServiceApplication>(applicationInfo.SearchServiceApplicationId);
            }

            return null;
        }

        /// <summary>
        /// Get the service application by its name
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>
        /// The search service application.
        /// </returns>
        public SearchServiceApplication GetSearchServiceApplicationByName(string appName)
        {
            var searchService = new SearchService("OSearch15", SPFarm.Local);
            var searchApplication = from SearchServiceApplication sapp in searchService.SearchApplications
                                    where sapp.GetSearchApplicationDisplayName() == appName
                                    select sapp;

            var serviceApp = searchApplication.First();

            return serviceApp;
        }

        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope\"s display group</param>
        /// <param name="searchPagePath">The scope\"s custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetScopesForSite(new Uri(site.Url)).Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);

            // only add if the scope doesn\"t exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, new Uri(site.Url), true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn\"t exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath)
        {
            // remotescopes class retrieves information via search web service so we run this as the search service account
            RemoteScopes remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));

            // see if there is an existing scope
            Scope scope = remoteScopes.GetSharedScopes().Cast<Scope>().FirstOrDefault(s => s.Name == scopeName);

            // only add if the scope doesn\"t exist already
            if (scope == null)
            {
                scope = remoteScopes.AllScopes.Create(scopeName, string.Empty, null, true, searchPagePath, ScopeCompilationType.AlwaysCompile);
            }

            // see if there is an existing display group         
            ScopeDisplayGroup displayGroup = remoteScopes.GetDisplayGroupsForSite(new Uri(site.Url)).Cast<ScopeDisplayGroup>().FirstOrDefault(d => d.Name == displayGroupName);

            // add if the display group doesn\"t exist
            if (displayGroup == null)
            {
                displayGroup = remoteScopes.AllDisplayGroups.Create(displayGroupName, string.Empty, new Uri(site.Url), true);
            }

            // add scope to display group if not already added
            if (!displayGroup.Contains(scope))
            {
                displayGroup.Add(scope);
                displayGroup.Update();
            }

            // optionally force a scope compilation so this is available immediately
            remoteScopes.StartCompilation();

            return scope;
        }

        /// <summary>
        /// Ensure a managed property in the search service application schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        /// <returns>The managed property</returns>
        public ManagedProperty EnsureManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo)
        {
            var ssa = this.GetDefaultSearchServiceApplication(site);
            var propertyName = managedPropertyInfo.Name;

            // this forces managed prop definition to SSA-scope 
            // (i.e. all managed props will be farm-wide)
            var owner = new SearchObjectOwner(SearchObjectLevel.Ssa, site.RootWeb);

            // Get the managed property and if null, create it
            var managedPropertyDefinition = this.GetManagedProperty(managedPropertyInfo, ssa, owner);
            if ((managedPropertyDefinition == null) || ShouldRecreateManagedProperty(managedPropertyDefinition, managedPropertyInfo))
            {
                // If managed property was created, make sure it sets the crawled property mappings and configuration
                managedPropertyInfo.UpdateBehavior = ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists;
                managedPropertyDefinition = this.CreateManagedProperty(managedPropertyInfo, ssa, owner);
            }

            if (managedPropertyDefinition != null)
            {
                // If crawled property mappings need to be overwritten or appended
                if (ShouldUpdateCrawledPropertyMappings(managedPropertyInfo))
                {
                    var mappings = GetInitialCrawledPropertyMappings(managedPropertyDefinition, managedPropertyInfo, ssa, owner);
                    this.SetCrawledPropertyMappings(site, managedPropertyDefinition, managedPropertyInfo, ssa, owner, mappings);
                }

                if (ShouldUpdateConfiguration(managedPropertyInfo))
                {
                    this.ConfigureManagerProperty(managedPropertyDefinition, managedPropertyInfo);
                }

                // Save through the schema manager (don't call .Update on the managed property object itself, its config won't get saved properly)
                ssa.UpdateManagedProperty(managedPropertyDefinition, owner);
            }

            // Re-fetch schema, it might be stale at this point
            var sspSchema = new Schema(ssa);
            return sspSchema.AllManagedProperties[propertyName];
        }

        /// <summary>
        /// Delete a managed property from the search schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        public void DeleteManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo)
        {
            var ssa = this.GetDefaultSearchServiceApplication(site);

            // Get the search schema
            var sspSchema = new Schema(ssa);
            var managedProperties = sspSchema.AllManagedProperties;

            if (managedProperties.Contains(managedPropertyInfo.Name))
            {
                var prop = managedProperties[managedPropertyInfo.Name];
                prop.DeleteAllMappings();
                prop.Delete();
            }
        }

        /// <summary>
        /// Gets the result source by name using the default search service application
        /// </summary>
        /// <param name="site">The site collection.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="scopeOwnerLevel">The level of the scope's owner.</param>
        /// <returns>
        /// The corresponding result source.
        /// </returns>
        public ISource GetResultSourceByName(SPSite site, string resultSourceName, SearchObjectLevel scopeOwnerLevel)
        {
            var serviceApplicationOwner = new SearchObjectOwner(scopeOwnerLevel, site.RootWeb);

            var context = SPServiceContext.GetContext(site);
            var searchProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy)) as SearchServiceApplicationProxy;

            return searchProxy.GetResultSourceByName(resultSourceName, serviceApplicationOwner);
        }

        /// <summary>
        /// Ensure a result source
        /// </summary>
        /// <param name="contextSite">The context SPSite object</param>
        /// <param name="resultSourceInfo">The result source configuration object</param>
        /// <returns>The name of the result source</returns>
        public Source EnsureResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo)
        {
            Source resultSource = null;
            var updateMode = resultSourceInfo.UpdateMode;

            var sortCollection = new SortCollection();

            if (resultSourceInfo.SortSettings != null)
            {            
                foreach (var sortSetting in resultSourceInfo.SortSettings)
                {
                    sortCollection.Add(sortSetting.Key, sortSetting.Value);
                }
            }

            var queryProperties = new QueryTransformProperties();
            queryProperties["SortList"] = sortCollection;

            // If the SortCollection contains "Rank" as one of its keys, specifiy the ranking model to be used. If a ranking model is
            // specified but sorting by Rank is not in the sort setting, throw an exception.
            if (resultSourceInfo.RankingModelId != Guid.Empty)
            {
                if ((resultSourceInfo.SortSettings != null && !resultSourceInfo.SortSettings.ContainsKey(BuiltInManagedProperties.Rank.Name)) || resultSourceInfo.SortSettings == null)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "You can't specify a ranking model id ({0}) if you are not sorting by rank. Make sure to include Rank as the first Sorting Key in the sort settings if you want to use a ranking model.",
                            resultSourceInfo.RankingModelId));
                }

                queryProperties["RankingModelId"] = resultSourceInfo.RankingModelId.ToString();
            }
            else if (resultSourceInfo.SortSettings != null && resultSourceInfo.SortSettings.ContainsKey(BuiltInManagedProperties.Rank.Name))
            {
                queryProperties["RankingModelId"] = BuiltInRankingModels.DefaultSearchModelId.ToString();
            }

            // Get the search service application for the current site
            var searchServiceApplication = this.GetDefaultSearchServiceApplication(contextSite);
            if (searchServiceApplication != null)
            {
                if (updateMode.Equals(ResultSourceUpdateBehavior.OverwriteResultSource))
                {
                    resultSource = InnerEnsureResultSource(
                        searchServiceApplication, 
                        resultSourceInfo.Name, 
                        resultSourceInfo.Level, 
                        resultSourceInfo.SearchProvider, 
                        contextSite.RootWeb, 
                        resultSourceInfo.Query,
                        queryProperties, 
                        true, 
                        resultSourceInfo.IsDefaultResultSourceForOwner);
                }
                else
                {
                    resultSource = InnerEnsureResultSource(
                        searchServiceApplication, 
                        resultSourceInfo.Name, 
                        resultSourceInfo.Level, 
                        resultSourceInfo.SearchProvider, 
                        contextSite.RootWeb, 
                        resultSourceInfo.Query,
                        queryProperties, 
                        false, 
                        resultSourceInfo.IsDefaultResultSourceForOwner);

                    var searchQuery = string.Empty;
                    if (updateMode.Equals(ResultSourceUpdateBehavior.OverwriteQuery))
                    {
                        searchQuery = resultSourceInfo.Query;
                    }
                    else if (updateMode.Equals(ResultSourceUpdateBehavior.AppendToQuery))
                    {
                        if (resultSource.QueryTransform != null)
                        {
                            // Check if appended query is already found on the current result source query template
                            // Note: remain case sensitive because the revert query option is also case sensitive.
                            if (!resultSource.QueryTransform.QueryTemplate.Contains(resultSourceInfo.Query))
                            {
                                searchQuery = resultSource.QueryTransform.QueryTemplate + " " + resultSourceInfo.Query;
                            }
                        }
                        else
                        {
                            searchQuery = resultSourceInfo.Query;
                        }
                    }
                    else if (updateMode.Equals(ResultSourceUpdateBehavior.RevertQuery))
                    {
                        if (resultSource.QueryTransform != null)
                        {
                            searchQuery = resultSource.QueryTransform.QueryTemplate.Replace(resultSourceInfo.Query, string.Empty).Trim();
                        }
                    }

                    resultSource.CreateQueryTransform(queryProperties, searchQuery);
                    resultSource.Commit();
                }
            }

            return resultSource;
        }

        /// <summary>
        /// Delete a result source
        /// </summary>
        /// <param name="contextSite">The context site collection</param>
        /// <param name="resultSourceInfo">The result source info object</param>
        public void DeleteResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo)
        {
            // Get the search service application for the current site
            this.DeleteResultSource(contextSite, resultSourceInfo.Name, resultSourceInfo.Level);
        }

        /// <summary>
        /// Deletes the result source.
        /// </summary>
        /// <param name="contextSite">Current site collection</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="level">The level.</param>
        public void DeleteResultSource(SPSite contextSite, string resultSourceName, SearchObjectLevel level)
        {
            var searchApp = this.GetDefaultSearchServiceApplication(contextSite);
            var federationManager = new FederationManager(searchApp);
            var searchOwner = new SearchObjectOwner(level, contextSite.RootWeb);

            var resultSource = federationManager.GetSourceByName(resultSourceName, searchOwner);
            if (resultSource != null)
            {
                federationManager.RemoveSource(resultSource);
            }
        }

        /// <summary>
        /// Ensure a Result Type in a site collection
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="resultType">The result type info object</param>
        /// <returns>The result type item</returns>
        public ResultItemType EnsureResultType(SPSite site, ResultTypeInfo resultType)
        {
            var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, site.RootWeb);
            var resultSource = this.GetResultSourceByName(site, resultType.ResultSource.Name, resultType.ResultSource.Level);

            var resultTypeManager = new ResultItemTypeManager(this.GetDefaultSearchServiceApplication(site));
            var existingResultTypes = resultTypeManager.GetResultItemTypes(searchOwner, true);

            // Get the existing result type
            var resType = existingResultTypes.FirstOrDefault(r => r.Name.Equals(resultType.Name));

            if (resType == null)
            {
                resType = new ResultItemType(searchOwner);
                resType.Name = resultType.Name;
                resType.SourceID = resultSource.Id;

                resType.DisplayTemplateUrl = resultType.DisplayTemplate.ItemTemplateTokenizedPath;
                var properties = resultType.DisplayProperties.Select(t => t.Name).ToArray();
                resType.DisplayProperties = string.Join(",", properties);
                resType.RulePriority = resultType.Priority;

                // Create rules
                var rules = 
                    resultType.Rules.Select(
                        this.CreateCustomPropertyRule)
                        .ToList();
                resType.Rules = new PropertyRuleCollection(rules);

                typeof(ResultItemType).GetProperty("OptimizeForFrequentUse")
                    .SetValue(resType, resultType.OptimizeForFrequenUse);

                // Add the result type
                resultTypeManager.AddResultItemType(resType);
            }
 
            return resType;
        }

        /// <summary>
        /// Delete a result type in the site collection
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="resultType">The result type object</param>
        public void DeleteResultType(SPSite site, ResultTypeInfo resultType)
        {
            ResultItemType resType = null;
    
            var searchOwner = new SearchObjectOwner(SearchObjectLevel.SPSite, site.RootWeb);
            var resultTypeManager = new ResultItemTypeManager(this.GetDefaultSearchServiceApplication(site));
            var existingResultTypes = resultTypeManager.GetResultItemTypes(searchOwner, true);

            // Get the existing result type
            resType = existingResultTypes.FirstOrDefault(r => r.Name.Equals(resultType.Name));

            if (resType != null)
            {
                resultTypeManager.DeleteResultItemType(resType);
            }  
        }

        /// <summary>
        /// Create a custom result type rule
        /// </summary>
        /// <param name="resultTypeRule">The result type rule info object</param>
        /// <returns>The property rule</returns>
        public PropertyRule CreateCustomPropertyRule(ResultTypeRuleInfo resultTypeRule)
        {
            var type = typeof(PropertyRuleOperator);
            var info = type.GetProperty("DefaultOperators", BindingFlags.NonPublic | BindingFlags.Static);
            var value = info.GetValue(null);
            var defaultOperators = (Dictionary<PropertyRuleOperator.DefaultOperator, PropertyRuleOperator>)value;

            var rule = new PropertyRule(resultTypeRule.PropertyName, defaultOperators[resultTypeRule.Operator])
            {
                PropertyValues = new List<string>(resultTypeRule.Values)
            };

            return rule;
        }

        /// <summary>
        /// Add faceted navigation refiners for a taxonomy term and its reuses
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="navigationInfo">The faceted navigation configuration object</param>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We want lowercase here.")]
        public void AddFacetedRefinersForTerm(SPSite site, FacetedNavigationInfo navigationInfo)
        {
            Term termItem;

            // Get the term from the term set if specified (supports reused terms in other term sets)
            // Else, default to the source term set to find the term.
            if (navigationInfo.Term.TermSet != null)
            {
                termItem = this.taxonomyService.GetTermForIdInTermSet(
                    site,
                    navigationInfo.Term.TermSet.Group.Name,
                    navigationInfo.Term.TermSet.Label,
                    navigationInfo.Term.Id);
            }
            else
            {
                termItem = this.taxonomyService.GetTermForId(site, navigationInfo.Term.Id);
            }

            // Flag the term set to use faceted navigation
            termItem.TermSet.SetCustomProperty("_Sys_Facet_IsFacetedTermSet", "True");
            termItem.TermStore.CommitAll();

            var termList = new List<Term> { termItem };

            termList.AddRange(termItem.ReusedTerms);

            foreach (var term in termList)
            {
                var i = 0;
                var fullRefinementString = new List<string>();
                var refinementConfig = new List<string>();

                foreach (var refiner in navigationInfo.Refiners)
                {
                    refinementConfig.Add("\"_Sys_Facet_RefinerConfig" + i + "\"");
                    fullRefinementString.Add(refiner.RefinementString);

                    var fp = CultureInfo.InvariantCulture;

                    var sb = new List<string>
                    {
                        string.Format(fp, "\"sortBy\":{0}", Convert.ChangeType(refiner.SortBy, refiner.SortBy.GetTypeCode(), fp)),
                        string.Format(fp, "\"sortOrder\":{0}", Convert.ChangeType(refiner.SortOrder, refiner.SortOrder.GetTypeCode(), fp)),
                        string.Format(fp, "\"maxNumberRefinementOptions\":{0}", refiner.MaxNumberRefinementOptions),
                        string.Format(fp, "\"propertyName\":\"{0}\"", refiner.ManagedPropertyName),
                        string.Format(fp, "\"type\":\"{0}\"", refiner.RefinerType),
                        string.Format(fp, "\"displayTemplate\":\"{0}\"", refiner.DisplayTemplateJsLocation),
                        string.Format(fp, "\"displayName\":\"{0}\"", refiner.DisplayName),
                        string.Format(fp, "\"useDefaultDateIntervals\":{0}", refiner.UseDefaultDateIntervals.ToString().ToLowerInvariant()),
                        string.Format(fp, "\"aliases\":{0}", refiner.Alias),
                        string.Format(fp, "\"refinerSpecStringOverride\":\"{0}\"", refiner.RefinerSpecStringOverride.ToLowerInvariant()),
                        string.Format(fp, "\"intervals\":{0}", refiner.Intervals)
                    };

                    term.SetCustomProperty("_Sys_Facet_RefinerConfig" + i, "{" + string.Join(",", sb.ToArray()) + "}");

                    i++;
                }

                term.SetCustomProperty("_Sys_Facet_FullRefinementString", string.Join(",", fullRefinementString.ToArray()));
                term.SetCustomProperty("_Sys_Facet_RefinementConfig",  "[" + string.Join(",", refinementConfig.ToArray()) + "]");

                term.TermStore.CommitAll();
            }
        }

        /// <summary>
        /// Deletes all refiners for the specified term and its reuses regardless previous configuration
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="term">The term info object</param>
        public void RemoveFacetedRefinersForTerm(SPSite site, TermInfo term)
        {
            // Get the term
            var termItem = this.taxonomyService.GetTermForId(site, term.Id);

            var properties = new[] { "_Sys_Facet_FullRefinementString", "_Sys_Facet_RefinementConfig", "_Sys_Facet_RefinerConfig" };

            foreach (var prop in properties)
            {
                string tmp;
                termItem.CustomProperties.TryGetValue(prop, out tmp);
                if (tmp != null)
                {
                    termItem.DeleteCustomProperty(prop);
                }
            }

            termItem.TermStore.CommitAll();
        }

        /// <summary>
        /// Ensure a search result source
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level.</param>
        /// <param name="searchProvider">The search provider for the result source.</param>
        /// <param name="contextWeb">The SPWeb to retrieve the search context.</param>
        /// <param name="query">The search query in KQL format.</param>
        /// <param name="properties">Query properties.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="isDefaultResultSourceForOwner">Whether this result source will be flagged as the default for the current search owner</param>
        /// <returns>
        /// The result source.
        /// </returns>
        private static Source InnerEnsureResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, string searchProvider, SPWeb contextWeb, string query, QueryTransformProperties properties, bool overwrite, bool isDefaultResultSourceForOwner)
        {
            var federationManager = new FederationManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            var resultSource = federationManager.GetSourceByName(resultSourceName, searchOwner);

            if (resultSource != null && overwrite)
            {
                federationManager.RemoveSource(resultSource);
            }

            if (resultSource == null || overwrite)
            {
                resultSource = federationManager.CreateSource(searchOwner);
                resultSource.Name = resultSourceName;
                resultSource.ProviderId = federationManager.ListProviders()[searchProvider].Id;
                resultSource.CreateQueryTransform(properties, query);
                resultSource.Commit();

                if (isDefaultResultSourceForOwner)
                {
                    federationManager.UpdateDefaultSource(resultSource.Id, searchOwner);
                }
            }

            return resultSource;
        }

        /// <summary>
        /// Get a crawled property by name
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="crawledPropertyName">The crawl property name</param>
        /// <returns>All crawled properties that match the name</returns>
        private IList<CrawledProperty> GetCrawledPropertyByName(SPSite site, string crawledPropertyName)
        {
            IList<CrawledProperty> crawledPropertiesMatchingName = new List<CrawledProperty>();

            var ssa = this.GetDefaultSearchServiceApplication(site);

            // Get the search schema
            var sspSchema = new Schema(ssa);

            // Search in all categories
            foreach (var category in sspSchema.AllCategories)
            {
                foreach (var property in category.GetAllCrawledProperties())
                {
                    if (string.CompareOrdinal(property.Name, crawledPropertyName) == 0)
                    {
                        crawledPropertiesMatchingName.Add(property);
                    }
                }
            }

            return crawledPropertiesMatchingName;
        }

        private SPManagedPropertyInfo GetManagedProperty(
            ManagedPropertyInfo managedPropertyInfo,
            SearchServiceApplication ssa,
            SearchObjectOwner owner)
        {
            SPManagedPropertyInfo managedPropertyDefinition = null;
            var propertyName = managedPropertyInfo.Name;

            // Get the search schema
            var sspSchema = new Schema(ssa);
            var managedProperties = sspSchema.AllManagedProperties;

            // If the managed property already exists
            // Else create it.
            if (managedProperties.Contains(propertyName))
            {
                managedPropertyDefinition = ssa.GetManagedProperty(propertyName, owner);
            }
            else
            {
                this.logger.Warn("Managed Property '{0}' not found.", propertyName);
            }

            return managedPropertyDefinition;
        }

        private SPManagedPropertyInfo CreateManagedProperty(
            ManagedPropertyInfo managedPropertyInfo,
            SearchServiceApplication ssa,
            SearchObjectOwner owner)
        {
            SPManagedPropertyInfo managedPropertyDefinition = null;
            var propertyName = managedPropertyInfo.Name;
            var propertyType = managedPropertyInfo.DataType;

            // Get the search schema
            var sspSchema = new Schema(ssa);
            var managedProperties = sspSchema.AllManagedProperties;

            // If the managed property already exists
            // Else create it.
            if (managedProperties.Contains(propertyName))
            {
                var prop = managedProperties[propertyName];
                if (prop.DeleteDisallowed)
                {
                    this.logger.Warn("Delete is disallowed on the Managed Property {0}", propertyName);
                }
                else
                {
                    prop.DeleteAllMappings();
                    prop.Delete();
                    managedPropertyDefinition = ssa.CreateManagedProperty(propertyName, propertyType, owner);
                }
            }
            else
            {
                managedPropertyDefinition = ssa.CreateManagedProperty(propertyName, propertyType, owner);
            }

            return managedPropertyDefinition;
        }

        private void SetCrawledPropertyMappings(
            SPSite site,
            SPManagedPropertyInfo managedPropertyDefinition,
            ManagedPropertyInfo managedPropertyInfo,
            SearchServiceApplication ssa,
            SearchObjectOwner owner,
            List<MappingInfo> mappings)
        {
            // Ensure crawl properties mappings
            foreach (var crawledPropertyKeyAndOrder in managedPropertyInfo.CrawledProperties)
            {
                // Get the crawled property (there may be more than one matching that name)
                var matchingCrawledProperties = this.GetCrawledPropertyByName(site, crawledPropertyKeyAndOrder.Key);
                if (matchingCrawledProperties != null && matchingCrawledProperties.Count > 0)
                {
                    foreach (var crawledProperty in matchingCrawledProperties)
                    {
                        // Create mapping information
                        var mapping = new MappingInfo
                        {
                            CrawledPropertyName = crawledProperty.Name,
                            CrawledPropset = crawledProperty.Propset,
                            ManagedPid = managedPropertyDefinition.Pid,
                            MappingOrder = crawledPropertyKeyAndOrder.Value
                        };

                        // If managed property doesn't already contain a mapping for the crawled property, add it
                        if (
                            ssa.GetManagedPropertyMappings(managedPropertyDefinition, owner)
                                .All(m => m.CrawledPropertyName != mapping.CrawledPropertyName))
                        {
                            mappings.Add(mapping);
                        }
                        else
                        {
                            this.logger.Info(
                                "Mapping for managed property {0} and crawled property with name {1} is already exists",
                                managedPropertyDefinition.Name,
                                crawledPropertyKeyAndOrder);
                        }
                    }
                }
                else
                {
                    this.logger.Warn("Crawled property with name {0} not found!", crawledPropertyKeyAndOrder);
                }
            }

            // Apply mappings to the managed property
            if (mappings.Count > 0)
            {
                ssa.SetManagedPropertyMappings(managedPropertyDefinition, mappings, owner);
            }
        }

        private void ConfigureManagerProperty(
            SPManagedPropertyInfo managedPropertyDefinition,
            ManagedPropertyInfo managedPropertyInfo)
        {
            if ((managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.UpdateConfiguration) || 
                (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists))
            {
                this.logger.Info(
                    "Configuring managed property '{0}' on update behavior '{1}'", 
                    managedPropertyInfo.Name, 
                    managedPropertyInfo.UpdateBehavior);

                // Configure the managed property
                managedPropertyDefinition.Sortable = managedPropertyInfo.Sortable;
                if (managedPropertyDefinition.Sortable)
                {
                    managedPropertyDefinition.SortableType = managedPropertyInfo.SortableType;
                }

                managedPropertyDefinition.Refinable = managedPropertyInfo.Refinable;
                if (managedPropertyDefinition.Refinable)
                {
                    // use "active" refine mode whenever refinable=TRUE
                    managedPropertyDefinition.RefinerConfiguration.Type =
                        Microsoft.Office.Server.Search.Administration.RefinerType.Deep;
                }

                managedPropertyDefinition.Retrievable = managedPropertyInfo.Retrievable;
                managedPropertyDefinition.RespectPriority = managedPropertyInfo.RespectPriority;
                managedPropertyDefinition.Queryable = managedPropertyInfo.Queryable;
                managedPropertyDefinition.Searchable = managedPropertyInfo.Searchable;

                if (managedPropertyDefinition.Searchable)
                {
                    managedPropertyDefinition.FullTextIndex = managedPropertyInfo.FullTextIndex;
                    managedPropertyDefinition.Context = managedPropertyInfo.Context;
                }
                else
                {
                    managedPropertyDefinition.FullTextIndex = string.Empty;
                    managedPropertyDefinition.Context = 0;
                }

                managedPropertyDefinition.HasMultipleValues = managedPropertyInfo.HasMultipleValues;
                managedPropertyDefinition.SafeForAnonymous = managedPropertyInfo.SafeForAnonymous;
            }
        }

        private static List<MappingInfo> GetInitialCrawledPropertyMappings(
            SPManagedPropertyInfo managedPropertyDefinition,
            ManagedPropertyInfo managedPropertyInfo,
            SearchServiceApplication ssa,
            SearchObjectOwner owner)
        {
            var mappingCollection = new List<MappingInfo>();

            // If specified to overwrite all crawled property mappings
            // set an empty mapping info list before recreating the mappings.
            // Else if, if specified to append the crawled properties, initialize the
            // mapping collection to the existing mappings on the managed property.
            switch (managedPropertyInfo.UpdateBehavior)
            {
                case ManagedPropertyUpdateBehavior.OverwriteCrawledProperties:
                case ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists:
                    ssa.SetManagedPropertyMappings(managedPropertyDefinition, mappingCollection, owner);
                    break;
                case ManagedPropertyUpdateBehavior.AppendCrawledProperties:
                    mappingCollection = ssa.GetManagedPropertyMappings(managedPropertyDefinition, owner);
                    break;
            }

            return mappingCollection;
        }

        private static bool ShouldUpdateConfiguration(ManagedPropertyInfo managedPropertyInfo)
        {
            return (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.UpdateConfiguration) ||
                   (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists);
        }

        private static bool ShouldUpdateCrawledPropertyMappings(ManagedPropertyInfo managedPropertyInfo)
        {
            return (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.OverwriteCrawledProperties)
                   || (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.AppendCrawledProperties)
                   || (managedPropertyInfo.UpdateBehavior == ManagedPropertyUpdateBehavior.OverwriteIfAlreadyExists);
        }

        private static bool ShouldRecreateManagedProperty(SPManagedPropertyInfo spManagedPropertyInfo, ManagedPropertyInfo managedPropertyInfo)
        {
            // If the managed type has changed, the managed property needs to be recreated.
            // NOTE: Simply changing the 'ManagedType' property doesn't work EVEN IF IT'S NOT READ-ONLY.
            return spManagedPropertyInfo.ManagedType != managedPropertyInfo.DataType;
        }
    }
}
