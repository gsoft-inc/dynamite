namespace GSoft.Dynamite.Navigation
{
    using System.Collections.Generic;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing.Navigation;

    /// <summary>
    /// Service for main menu navigation nodes.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Get the pages tagged with terms across the search service
        /// </summary>
        /// <param name="properties">The Managed Properties</param>
        /// <returns>Navigation node</returns>
        IEnumerable<NavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties);

        /// <summary>
        /// Get the pages tagged with terms across the search service
        /// </summary>
        /// <param name="properties">The Managed Properties</param>
        /// <param name="filteredContentTypeId">The content type id</param>
        /// <param name="occurrenceValue">The location of items</param>
        /// <param name="term">The current term</param>
        /// <returns>Navigation node</returns>
        IEnumerable<NavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties, string filteredContentTypeId, string occurrenceValue, string term);

        /// <summary>
        /// Map nodes with items
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <param name="navigationItems">Navigation Items</param>
        /// <returns>Navigation nodes</returns>
        IEnumerable<NavigationNode> MapNavigationNodeTree(IEnumerable<NavigationNode> navigationTerms, IEnumerable<NavigationNode> navigationItems);

        /// <summary>
        /// Gets the navigation node terms.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="properties">The Managed Properties</param>
        /// <param name="navigationTerms">The navigation terms.</param>
        /// <returns>A navigation node tree.</returns>
        IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, NavigationManagedProperties properties, IEnumerable<NavigationTerm> navigationTerms);

        /// <summary>
        /// Gets all navigation node terms
        /// </summary>
        /// <param name="web">The current web</param>
        /// <param name="properties">The navigation managed properties</param>
        /// <param name="navigationTerms">The navigation terms</param>
        /// <param name="maxLevel">the max level</param>
        /// <returns>The node terms</returns>
        IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, NavigationManagedProperties properties, IEnumerable<NavigationTerm> navigationTerms, int maxLevel);

        /// <summary>
        /// Gets all the navigation terms.
        /// </summary>
        /// <param name="web">The Current web</param>
        /// <param name="properties">The navigation properties</param>
        /// <returns>List of navigation node</returns>
        IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationManagedProperties properties);
    }
}