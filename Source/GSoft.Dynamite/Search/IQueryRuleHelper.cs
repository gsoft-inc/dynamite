using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Taxonomy;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Administration.Query;
using Microsoft.Office.Server.Search.Query;
using Microsoft.Office.Server.Search.Query.Rules;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Search
{
    /// <summary>
    /// Search query rule utilities
    /// </summary>
    public interface IQueryRuleHelper
    {
        /// <summary>
        /// Get all query rules matching the display name in the search level
        /// </summary>
        /// <param name="contextSite">The current site collection.</param>
        /// <param name="displayName">The query rule display name.</param>
        /// <param name="level">The search level.</param>
        /// <returns>A list of query rules</returns>
        ICollection<QueryRule> GetQueryRulesByName(SPSite contextSite, string displayName, SearchObjectLevel level);

        /// <summary>
        /// Creates a query rule object for the search level.
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="queryRuleMetadata">The query rule definition.</param>
        /// <param name="level">The search level object.</param>
        /// <returns>The new query rule object.</returns>
        QueryRule EnsureQueryRule(SPSite site, QueryRuleInfo queryRuleMetadata, SearchObjectLevel level);

        /// <summary>
        /// Delete all query rules corresponding to the display name
        /// </summary>
        /// <param name="site">The current site collection.</param>
        /// <param name="displayName">The query rule name.</param>
        /// <param name="level">The search level.</param>
        void DeleteQueryRule(SPSite site, string displayName, SearchObjectLevel level);

        /// <summary>
        /// Ensure a search best bet
        /// </summary>
        /// <param name="site">The current site collection</param>
        /// <param name="bestBetDefinition">The best best metadata</param>
        /// <param name="level">The search object level.</param>
        /// <returns>The best bet object.</returns>
        Microsoft.Office.Server.Search.Query.Rules.BestBet EnsureBestBet(SPSite site, BestBetInfo bestBetDefinition, SearchObjectLevel level);

        /// <summary>
        /// Create a promoted link action for a a query rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="bestBetId">The bestBetIds</param>
        void CreatePromotedResultAction(QueryRule rule, Guid bestBetId);

        /// <summary>
        /// Create a change query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        void CreateChangeQueryAction(QueryRule rule, string queryTemplate, Guid resultSourceId);

        /// <summary>
        /// Create a result block query action for a Query Rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="blockTitle">The result block Title</param>
        /// <param name="queryTemplate">The search query template in KQL format</param>
        /// <param name="resultSourceId">The search result source Id</param>
        /// <param name="routingLabel">A routing label for a content search WebPart</param>
        /// <param name="numberOfItems">The number of result to retrieve</param>
        void CreateResultBlockAction(QueryRule rule, string blockTitle, string queryTemplate, Guid resultSourceId, string routingLabel, string numberOfItems);
    }
}