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
        IEnumerable<NavigationNode> GetNavigationNodeItems(NavigationManagedProperties properties);

        IEnumerable<NavigationNode> MapNavigationNodeTree(IEnumerable<NavigationNode> navigationTerms, IEnumerable<NavigationNode> navigationItems);

        IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, IEnumerable<NavigationTerm> navigationTerms);

        IEnumerable<NavigationNode> GetNavigationNodeTerms(SPWeb web, IEnumerable<NavigationTerm> navigationTerms, int maxLevel);

        IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationManagedProperties properties);
    }
}