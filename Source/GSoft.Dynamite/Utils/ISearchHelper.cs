namespace GSoft.Dynamite.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using GSoft.Dynamite.Definitions;

    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.Office.Server.Search.Administration.Query;
    using Microsoft.Office.Server.Search.Query;
    using Microsoft.Office.Server.Search.Query.Rules;
    using Microsoft.SharePoint;

    public interface ISearchHelper
    {
        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope's display group</param>
        /// <param name="searchPagePath">The scope's custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>The ensure result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        /// <returns>The <see cref="ResultItemType"/>.</returns>
        ResultItemType EnsureResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>
        /// Gets the result source by name using the default application name:'Search Service Application'.
        /// </summary>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="site">The site collection.</param>
        /// <param name="owner">The owner.</param>
        /// <returns>
        /// The corresponding result source.
        /// </returns>
        SourceRecord GetResultSourceByName(string resultSourceName, SPSite site, SearchObjectLevel owner);

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
        /// <returns>
        /// The result source.
        /// </returns>
        Source EnsureResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, string searchProvider, SPWeb contextWeb, string query, QueryTransformProperties properties, bool overwrite);

        /// <summary>
        /// Get a result source object by name
        /// </summary>
        /// <param name="ssa">The search service application</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level</param>
        /// <param name="contextWeb">The web context</param>
        /// <returns>The source object</returns>
        Source GetResultSourceByName(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, SPWeb contextWeb);

        /// <summary>
        /// Ensure a search result source
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level.</param>
        /// <param name="searchProvider">The search provider for this result source.</param>
        /// <param name="contextWeb">The SPWeb to retrieve the search context.</param>
        /// <param name="query">The search query in KQL format.</param>
        /// <param name="sortField">Internal name of the sort field.</param>
        /// <param name="direction">The sort direction.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns>
        /// The result source.
        /// </returns>
        Source EnsureResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, string searchProvider, SPWeb contextWeb, string query, string sortField, SortDirection direction, bool overwrite);

        /// <summary>The ensure result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        /// <returns>The <see cref="Source"/>.</returns>
        Source EnsureResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>
        /// Ensure a search result source
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">The result source name</param>
        /// <param name="level">The search object level.</param>
        /// <param name="searchProvider">The search provider for this result source.</param>
        /// <param name="contextWeb">The SPWeb to retrieve the search context.</param>
        /// <param name="query">The search query in KQL format.</param>
        /// <param name="sortFields">Internal name of the sort field.</param>
        /// <param name="directions">The sort direction.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns>
        /// The result source.
        /// </returns>
        Source EnsureResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, string searchProvider, SPWeb contextWeb, string query, IEnumerable<string> sortFields, IEnumerable<SortDirection> directions, bool overwrite);

        /// <summary>
        /// Get the service application by its name
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>
        /// The search service application.
        /// </returns>
        SearchServiceApplication GetDefaultSearchServiceApplication(string appName);

        /// <summary>
        /// Gets the default search service application from a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The search service application.</returns>
        SearchServiceApplication GetDefaultSearchServiceApplication(SPSite site);

        /// <summary>
        /// Deletes the result source.
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="level">The level.</param>
        /// <param name="contextWeb">The context web.</param>
        void DeleteResultSource(SearchServiceApplication ssa, string resultSourceName, SearchObjectLevel level, SPWeb contextWeb);

        /// <summary>The delete result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        void DeleteResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>The delete result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        void DeleteResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>The delete managed property.</summary>
        /// <param name="site">The site.</param>
        /// <param name="managedPropertyInfo">The managed property info.</param>
        void DeleteManagedProperty(SPSite site, Definitions.ManagedPropertyInfo managedPropertyInfo);

        /// <summary>
        /// Creates a query rule object for the search level.
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search level object.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="isActive">True if the query is active. False otherwise.</param>
        /// <param name="startDate">The query rule publishing start date.</param>
        /// <param name="endDate">The query rule publishing end date.</param>
        /// <returns>The new query rule object.</returns>
        QueryRule CreateQueryRule(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName, bool isActive, DateTime? startDate, DateTime? endDate);

        /// <summary>
        /// Ensure a search best bet
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search object level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="title">The title of the best bet.</param>
        /// <param name="url">The url of the best bet.</param>
        /// <param name="description">The description of the best bet.</param>
        /// <param name="isVisualBestBet">True if it is a visual best bet. False otherwise.</param>
        /// <param name="deleteIfUnused">True if must be deleted if unused. False otherwise.</param>
        /// <returns>The best bet object.</returns>
        Microsoft.Office.Server.Search.Query.Rules.BestBet EnsureBestBet(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string title, Uri url, string description, bool isVisualBestBet, bool deleteIfUnused);

        /// <summary>
        /// Delete all query rules corresponding to the display name
        /// </summary>
        /// <param name="ssa">The search service application.</param>
        /// <param name="level">The search level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The query rule name.</param>
        void DeleteQueryRule(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName);

        /// <summary>
        /// Get all query rules matching the display name in the search level
        /// </summary>
        /// <param name="ssa">The search service.</param>
        /// <param name="level">The search level.</param>
        /// <param name="contextWeb">The SPWeb context.</param>
        /// <param name="displayName">The query rule display name.</param>
        /// <returns>A list of query rules</returns>
        List<QueryRule> GetQueryRulesByName(SearchServiceApplication ssa, SearchObjectLevel level, SPWeb contextWeb, string displayName);

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

        /// <summary>
        /// Create a promoted link action for a a query rule
        /// </summary>
        /// <param name="rule">The query rule object</param>
        /// <param name="bestBetId">The bestBetIds</param>
        void CreatePromotedResultAction(QueryRule rule, Guid bestBetId);

        PropertyRule CreateCustomPropertyRule(ResultTypeRuleInfo resultTypeRule);
    }
}