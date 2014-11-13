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
        /// Gets all the navigation terms.
        /// </summary>
        /// <param name="web">The Current web</param>
        /// <param name="properties">The navigation properties</param>
        /// <returns>List of navigation node</returns>
        IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationManagedProperties properties);
    }
}