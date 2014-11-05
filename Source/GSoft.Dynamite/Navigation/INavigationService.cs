namespace GSoft.Dynamite.Navigation
{
    using System.Collections.Generic;

    using Microsoft.SharePoint.Publishing.Navigation;
    using Microsoft.SharePoint;

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
        IEnumerable<INavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties);

        /// <summary>
        /// Map nodes with items
        /// </summary>
        /// <param name="navigationTerms">Navigation terms</param>
        /// <param name="navigationItems">Navigation Items</param>
        /// <returns>Navigation nodes</returns>
        IEnumerable<INavigationNode> MapNavigationNodeTree(IEnumerable<INavigationNode> navigationTerms, IEnumerable<INavigationNode> navigationItems);

        IEnumerable<INavigationNode> GetNavigationNodeTerms(SPWeb web, IEnumerable<NavigationTerm> navigationTerms);

        IEnumerable<INavigationNode> GetNavigationNodeTerms(SPWeb web, IEnumerable<NavigationTerm> navigationTerms, int maxLevel);

        IEnumerable<INavigationNode> GetAllNavigationNodes(SPWeb web, NavigationManagedProperties properties);

    }
}