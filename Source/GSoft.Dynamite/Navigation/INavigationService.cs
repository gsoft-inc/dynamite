using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Navigation
{
    /// <summary>
    /// Service for main menu navigation nodes.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets all the navigation terms.
        /// </summary>
        /// <param name="web">The Current web</param>
        /// <param name="queryParameters">The navigation query parameters.</param>
        /// <returns>
        /// List of navigation node.
        /// </returns>
        IEnumerable<NavigationNode> GetAllNavigationNodes(SPWeb web, NavigationQueryParameters queryParameters);
    }
}