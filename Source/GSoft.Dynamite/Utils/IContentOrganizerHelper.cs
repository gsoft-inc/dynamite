namespace GSoft.Dynamite.Utils
{
    using Microsoft.SharePoint;

    public interface IContentOrganizerHelper
    {
        /// <summary>
        /// Create a custom rule for the content organizer (Without folder auto creation)
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="ruleName">The rule name</param>
        /// <param name="ruleDescription">The rule description</param>
        /// <param name="contentType">The content type name</param>
        /// <param name="conditionXml">Conditions in XML</param>
        /// <param name="priority">Rule priority</param>
        /// <param name="routeToExternalLocation">Route to external location</param>
        /// <param name="targetPath">The target path</param>
        /// <param name="customerRouterName">Custom router name</param>
        void CreateCustomRule(
            SPWeb web,
            string ruleName,
            string ruleDescription,
            string contentType,
            string conditionXml,
            int priority,
            bool routeToExternalLocation,
            string targetPath,
            string customerRouterName);

        /// <summary>
        /// Delete a custom rule
        /// </summary>
        /// <param name="web">the web</param>
        /// <param name="ruleName">The rule name</param>
        void DeleteCustomRule(SPWeb web, string ruleName);

        /// <summary>
        /// Delete a custom router
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="routerName">The router name</param>
        void DeleteCustomRouter(SPWeb web, string routerName);

        /// <summary>
        /// Create a custom router for a web content organizer
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="routerName">Router name</param>
        /// <param name="routerAssemblyName">Router assembly name</param>
        /// <param name="routerClassName">Router class name in the assembly</param>
        void CreateCustomRouter(SPWeb web, string routerName, string routerAssemblyName, string routerClassName);

        /// <summary>
        /// Get the content organizer Drop Off Library for a SPWeb
        /// </summary>
        /// <param name="web">The web</param>
        /// <returns>The Drop Off Library list</returns>
        SPList GetDropOffLibrary(SPWeb web);

        /// <summary>
        /// Force content organizer timer job to run
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        void ForceContentOrganizerTimerJobProcessing(SPSite site);
    }
}