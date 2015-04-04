using System.Collections.Generic;
using System.ServiceModel.Activation;
using Autofac;
using GSoft.Dynamite.Navigation;
using GSoft.Dynamite.WebServices;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Services
{
    /// <summary>
    /// A WCF REST service that manages navigation by taxonomy.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class NavigationWebService : INavigationWebService
    {
        /// <summary>
        /// Gets all navigation nodes based on the navigation query parameters.
        /// </summary>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns>A hierarchy of navigation nodes.</returns>
        public IEnumerable<NavigationNode> GetAllNavigationNodes(NavigationQueryParameters queryParameters)
        {
            INavigationService navigationService;
            SPWeb currentWeb = SPContext.Current.Web;

            using (var scope = DynamiteWspContainerProxy.BeginLifetimeScope(currentWeb))
            {
                navigationService = scope.Resolve<INavigationService>();
            }

            return navigationService.GetAllNavigationNodes(currentWeb, queryParameters);
        }
    }
}