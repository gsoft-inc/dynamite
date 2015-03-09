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
    /// Search service utilities
    /// </summary>
    public interface ISearchHelper
    {
        /// <summary>
        /// Gets the default search service application from a site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The search service application.</returns>
        SearchServiceApplication GetDefaultSearchServiceApplication(SPSite site);

        /// <summary>
        /// Get the service application by its name
        /// </summary>
        /// <param name="appName">Name of the application.</param>
        /// <returns>
        /// The search service application.
        /// </returns>
        SearchServiceApplication GetSearchServiceApplicationByName(string appName);

        /// <summary>
        /// Creates a site search scope if it doesn't exist yet
        /// </summary>
        /// <param name="site">The site collection</param>
        /// <param name="scopeName">The name of the search scope</param>
        /// <param name="displayGroupName">The scope's display group</param>
        /// <param name="searchPagePath">The scope's custom search page url (cannot be empty)</param>
        /// <returns>The search scope</returns>
        Scope EnsureSiteScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>
        /// Creates a farm-wide shared search scope
        /// </summary>
        /// <param name="site">The site collection of the context</param>
        /// <param name="scopeName">The name of the shared scope to create</param>
        /// <param name="displayGroupName">The search scope display group name</param>
        /// <param name="searchPagePath">Path to scope-specific search page</param>
        /// <returns>The newly created scope</returns>
        Scope EnsureSharedScope(SPSite site, string scopeName, string displayGroupName, string searchPagePath);

        /// <summary>
        /// Ensure a managed property in the search service application schema
        /// </summary>
        /// <param name="site">The context site</param>
        /// <param name="managedPropertyInfo">The managed property info</param>
        /// <returns>The managed property</returns>
        ManagedProperty EnsureManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo);

        /// <summary>The delete managed property.</summary>
        /// <param name="site">The site.</param>
        /// <param name="managedPropertyInfo">The managed property info.</param>
        void DeleteManagedProperty(SPSite site, ManagedPropertyInfo managedPropertyInfo);

        /// <summary>The ensure result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        /// <returns>The <see cref="ResultItemType"/>.</returns>
        ResultItemType EnsureResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>The delete result type.</summary>
        /// <param name="site">The site.</param>
        /// <param name="resultType">The result type.</param>
        void DeleteResultType(SPSite site, ResultTypeInfo resultType);

        /// <summary>
        /// Gets the result source by name using the default search service application
        /// </summary>
        /// <param name="site">The site collection.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="scopeOwnerLevel">The level of the scope's owner.</param>
        /// <returns>
        /// The corresponding result source.
        /// </returns>
        ISource GetResultSourceByName(SPSite site, string resultSourceName, SearchObjectLevel scopeOwnerLevel);

        /// <summary>The ensure result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        /// <returns>The <see cref="Source"/>.</returns>
        Source EnsureResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>The delete result source.</summary>
        /// <param name="contextSite">The context site.</param>
        /// <param name="resultSourceInfo">The result source info.</param>
        void DeleteResultSource(SPSite contextSite, ResultSourceInfo resultSourceInfo);

        /// <summary>
        /// Deletes the result source.
        /// </summary>
        /// <param name="contextSite">Current site collection.</param>
        /// <param name="resultSourceName">Name of the result source.</param>
        /// <param name="level">The level.</param>
        void DeleteResultSource(SPSite contextSite, string resultSourceName, SearchObjectLevel level);

        /// <summary>
        /// Creates a custom property rule
        /// </summary>
        /// <param name="resultTypeRule">The result type rule metadata</param>
        /// <returns>The created property rule</returns>
        PropertyRule CreateCustomPropertyRule(ResultTypeRuleInfo resultTypeRule);

        /// <summary>
        /// Add faceted navigation refiners for a taxonomy term and its reuses
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="navigationInfo">The faceted navigation configuration object</param>
        void AddFacetedRefinersForTerm(SPSite site, FacetedNavigationInfo navigationInfo);

        /// <summary>
        /// Deletes all refiners for the specified term and its reuses regardless previous configuration
        /// </summary>
        /// <param name="site">The site</param>
        /// <param name="term">The term info object</param>
        void RemoveFacetedRefinersForTerm(SPSite site, TermInfo term);
    }
}