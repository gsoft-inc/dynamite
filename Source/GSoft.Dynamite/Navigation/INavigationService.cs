namespace GSoft.Dynamite.Navigation
{
    using System.Collections.Generic;

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
        /// Get all navigation node terms
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <returns>navigation node terms</returns>
        IEnumerable<NavigationNode> GetNavigationNodeTerms(IEnumerable<NavigationTerm> navigationTerms);

        /// <summary>
        /// Map nodes with items
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <param name="navigationItems">Navigation Items</param>
        /// <returns>Navigation nodes</returns>
        IEnumerable<NavigationNode> MapNavigationNodeTree(IEnumerable<NavigationNode> navigationTerms, IEnumerable<NavigationNode> navigationItems);
    }
}