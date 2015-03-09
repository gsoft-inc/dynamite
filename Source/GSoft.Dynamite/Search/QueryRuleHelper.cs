using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Search.Enums;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.Utils;
using Microsoft.Office.Server.Auditing;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.JSGrid;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;
using Source = Microsoft.Office.Server.Search.Administration.Query.Source;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Search query rule utilities
    /// </summary>
    public class QueryRuleHelper : IQueryRuleHelper
    {
        private readonly ISearchHelper searchHelper;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="searchHelper">The search helper instance</param>
        public QueryRuleHelper(ISearchHelper searchHelper)
        {
            this.searchHelper = searchHelper;
        }
        
        /// <summary>
        /// Get all query rules matching the display name in the search level
        /// </summary>
        /// <param name="contextSite">The current site collection.</param>
        /// <param name="displayName">The query rule display name.</param>
        /// <param name="level">The search level.</param>
        /// <returns>A list of query rules</returns>
        public ICollection<QueryRule> GetQueryRulesByName(SPSite contextSite, string displayName, SearchObjectLevel level)
        {
            var queryRules = new List<QueryRule>();

            // Get all query rules for this level
            var searchApp = this.searchHelper.GetDefaultSearchServiceApplication(contextSite);
            var rules = GetQueryRules(searchApp, level, contextSite.RootWeb);

            if (rules.Contains(displayName))
            {
                queryRules = rules[displayName].ToList();
            }

            return queryRules;
        }
        
        /// <summary>
        /// Creates a query rule object for the search level.
        /// If the rule already exists, if may be overwritten, depending on
        /// the QueryRuleInfo upgrade behavior definition (the OverwriteIfAlreadyExists
        /// flag is false by default).
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="queryRuleMetadata">The query rule definition.</param>
        /// <param name="level">The search level object.</param>
        /// <returns>The new query rule object.</returns>
        public QueryRule EnsureQueryRule(SPSite site, QueryRuleInfo queryRuleMetadata, SearchObjectLevel level)
        {
            var searchApp = this.searchHelper.GetDefaultSearchServiceApplication(site);
            var queryRuleManager = new QueryRuleManager(searchApp);
            var searchOwner = new SearchObjectOwner(level, site.RootWeb);

            // Build the SearchObjectFilter
            var searchObjectFilter = new SearchObjectFilter(searchOwner);

            QueryRuleCollection rules = queryRuleManager.GetQueryRules(searchObjectFilter);

            QueryRule returnedRule = null;
            var existingRule = rules.FirstOrDefault(r => r.DisplayName == queryRuleMetadata.DisplayName);

            if (existingRule != null)
            {
                // Deal with upgrade behavior (delete and re-create or return existing)
                if (queryRuleMetadata.OverwriteIfAlreadyExists)
                {
                    rules.RemoveQueryRule(existingRule);
                    returnedRule = rules.CreateQueryRule(queryRuleMetadata.DisplayName, queryRuleMetadata.StartDate, queryRuleMetadata.EndDate, queryRuleMetadata.IsActive);
                }
                else
                {
                    returnedRule = existingRule;
                }
            }
            else
            {
                // None exist already with that display name, create it
                returnedRule = rules.CreateQueryRule(queryRuleMetadata.DisplayName, queryRuleMetadata.StartDate, queryRuleMetadata.EndDate, queryRuleMetadata.IsActive);
            }

            return returnedRule;
        }
        
        /// <summary>
        /// Delete all query rules corresponding to the display name
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="displayName">The query rule name.</param>
        /// <param name="level">The search level.</param>
        public void DeleteQueryRule(SPSite site, string displayName, SearchObjectLevel level)
        {
            // Get all query rules for this level
            var searchApp = this.searchHelper.GetDefaultSearchServiceApplication(site);
            var rules = GetQueryRules(searchApp, level, site.RootWeb);

            var queryRuleCollection = new List<QueryRule>();

            if (rules.Contains(displayName))
            {
                queryRuleCollection = rules[displayName].ToList();
            }

            if (queryRuleCollection.Count > 0)
            {
                foreach (var queryRule in queryRuleCollection)
                {
                    rules.RemoveQueryRule(queryRule);
                }
            }
        }
        
        /// <summary>
        /// Ensure a search best bet
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <param name="bestBetDefinition">The best best metadata</param>
        /// <param name="level">The search object level.</param>
        /// <returns>The best bet object.</returns>
        public Microsoft.Office.Server.Search.Query.Rules.BestBet EnsureBestBet(SPSite site, BestBetInfo bestBetDefinition, SearchObjectLevel level)
        {
            var searchApp = this.searchHelper.GetDefaultSearchServiceApplication(site);
            Microsoft.Office.Server.Search.Query.Rules.BestBet bestBet = null;
            var queryRuleManager = new QueryRuleManager(searchApp);
            var searchOwner = new SearchObjectOwner(level, site.RootWeb);

            // Build the SearchObjectFilter
            var searchObjectFilter = new SearchObjectFilter(searchOwner);

            var bestBets = queryRuleManager.GetBestBets(searchObjectFilter);

            if (!bestBets.Contains(bestBetDefinition.Url))
            {
                bestBet = bestBets.CreateBestBet(
                    bestBetDefinition.Title, 
                    bestBetDefinition.Url, 
                    bestBetDefinition.Description,
                    bestBetDefinition.IsVisualBestBet,
                    bestBetDefinition.DeleteIfUnused);
            }
            else
            {
                bestBet = bestBets[bestBetDefinition.Url];
            }

            return bestBet;
        }

        /// <summary>
        /// Get a crawled property by name
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="crawledPropertyName">The crawl property name</param>
        /// <returns>The crawled property</returns>
        public CrawledProperty GetCrawledPropertyByName(SPSite site, string crawledPropertyName)
        {
            CrawledProperty crawledProperty = null;

            var ssa = this.searchHelper.GetDefaultSearchServiceApplication(site);
            
            // Get the search schema
            var sspSchema = new Schema(ssa);

            // Search in all categories
            foreach (var category in sspSchema.AllCategories)
            {
                foreach (var property in category.GetAllCrawledProperties())
                {
                    if (string.CompareOrdinal(property.Name, crawledPropertyName) == 0)
                    {
                        crawledProperty = property;
                    }
                }
            }

            return crawledProperty;
        }

        /// <summary>
        /// Create a change query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        public void CreateChangeQueryAction(QueryRule rule, string queryTemplate, Guid resultSourceId)
        {
            var queryAction = (ChangeQueryAction)rule.CreateQueryAction(QueryActionType.ChangeQuery);

            if (!string.IsNullOrEmpty(queryTemplate))
            {
                queryAction.QueryTransform.QueryTemplate = queryTemplate;
            }

            queryAction.QueryTransform.SourceId = resultSourceId;

            rule.Update();
        }

        /// <summary>
        /// Create a result block query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="blockTitle">The result block Title</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        /// <param name="routingLabel">A routing label for a content search WebPart</param>
        /// <param name="numberOfItems">The number of result to retrieve</param>
        public void CreateResultBlockAction(QueryRule rule, string blockTitle, string queryTemplate, Guid resultSourceId, string routingLabel, string numberOfItems)
        {
            var queryAction = (CreateResultBlockAction)rule.CreateQueryAction(QueryActionType.CreateResultBlock);

            queryAction.ResultTitle.DefaultLanguageString = blockTitle;

            if (!string.IsNullOrEmpty(queryTemplate))
            {
                queryAction.QueryTransform.QueryTemplate = queryTemplate;
            }

            queryAction.QueryTransform.SourceId = resultSourceId;

            if (!string.IsNullOrEmpty(routingLabel))
            {
                queryAction.ResultTableType = routingLabel;
            }

            if (!string.IsNullOrEmpty(numberOfItems))
            {
                queryAction.QueryTransform.OverrideProperties = new QueryTransformProperties();
                queryAction.QueryTransform.OverrideProperties["RowLimit"] = int.Parse(numberOfItems, CultureInfo.InvariantCulture);
                queryAction.QueryTransform.OverrideProperties["TotalRowsExactMinimum"] = int.Parse(numberOfItems, CultureInfo.InvariantCulture);
            }

            rule.Update();
        }

        /// <summary>
        /// Create a promoted link action for a a query rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="bestBetId">The bestBetIds</param>
        public void CreatePromotedResultAction(QueryRule rule, Guid bestBetId)
        {
            var queryAction = (AssignBestBetsAction)rule.CreateQueryAction(QueryActionType.AssignBestBet);

            queryAction.BestBetIds.Add(bestBetId);

            rule.Update();
        }

        /// <summary>
        /// Get all query rules for a search level.
        /// </summary>
        /// <param name="ssa">The search service.</param>
        /// <param name="level">The search object level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <returns>A query rule collection.</returns>
        private static QueryRuleCollection GetQueryRules(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb)
        {
            var queryRuleManager = new QueryRuleManager(ssa);
            var searchOwner = new SearchObjectOwner(level, contextWeb);

            return queryRuleManager.GetQueryRules(new SearchObjectFilter(searchOwner));
        }
    }
}
