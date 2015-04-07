using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using GSoft.Dynamite.Navigation;

namespace GSoft.Dynamite.WebServices
{
    /// <summary>
    /// Interface to define a navigation WCF REST web service.
    /// </summary>
    [ServiceContract]
    public interface INavigationWebService
    {
        /// <summary>
        /// Gets all navigation nodes based on the navigation query parameters.
        /// </summary>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns>A hierarchy of navigation nodes.</returns>
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        IEnumerable<NavigationNode> GetAllNavigationNodes(NavigationQueryParameters queryParameters);
    }
}
