using System;
using System.Globalization;
using System.Linq;

using Microsoft.Office.RecordsManagement.RecordsRepository;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// SharePoint Content Organizer Helper
    /// </summary>
    public class ContentOrganizerHelper
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
        public void CreateCustomRule(
            SPWeb web,
            string ruleName,
            string ruleDescription,
            string contentType,
            string conditionXml,
            int priority,
            bool routeToExternalLocation,
            string targetPath,
            string customerRouterName)
        {
            var organizerRule = new EcmDocumentRouterRule(web)
                {
                    Name = ruleName,
                    Description = ruleDescription,
                    ContentTypeString = contentType,
                    ConditionsString = conditionXml,
                    TargetPath = targetPath,
                    RouteToExternalLocation = routeToExternalLocation,
                    Enabled = true,
                    Priority = priority.ToString(CultureInfo.InvariantCulture),
                    CustomRouter = customerRouterName,                  
                };

            // Create the rule
            organizerRule.Update();
        }

        /// <summary>
        /// Delete a custom rule
        /// </summary>
        /// <param name="web">the web</param>
        /// <param name="ruleName">The rule name</param>
        public void DeleteCustomRule(SPWeb web, string ruleName)
        {
            var routingWeb = new EcmDocumentRoutingWeb(web);
            var organizerRules = routingWeb.RoutingRuleCollection;

            var ruleToDelete = (from EcmDocumentRouterRule rule in organizerRules
                                where rule.Name.Equals(ruleName, StringComparison.Ordinal)
                                select rule).FirstOrDefault();

            if (ruleToDelete != null)
            {
                ruleToDelete.Delete();
            }
        }

        /// <summary>
        /// Delete a custom router
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="routerName">The router name</param>
        public void DeleteCustomRouter(SPWeb web, string routerName)
        {
            var contentOrganizer = new EcmDocumentRoutingWeb(web);
            contentOrganizer.RemoveCustomRouter(routerName);
        }

        /// <summary>
        /// Create a custom router for a web content organizer
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="routerName">Router name</param>
        /// <param name="routerAssemblyName">Router assembly name</param>
        /// <param name="routerClassName">Router class name in the assembly</param>
        public void CreateCustomRouter(SPWeb web, string routerName, string routerAssemblyName, string routerClassName)
        {
             var contentOrganizer = new EcmDocumentRoutingWeb(web);
             DeleteCustomRouter(web, routerName);
             contentOrganizer.AddCustomRouter(routerName, routerAssemblyName, routerClassName);
        }

        /// <summary>
        /// Get the content organizer Drop Off Library for a SPWeb
        /// </summary>
        /// <param name="web">The web</param>
        /// <returns>The Drop Off Library list</returns>
        public SPList GetDropOffLibrary(SPWeb web)
        {
            // Get the Drop Off Library
            var dropOffLibrary =
                (from SPList list in web.Lists
                 where list.RootFolder.Name.Equals("DropOffLibrary", StringComparison.Ordinal)
                 select list).FirstOrDefault();

            return dropOffLibrary;
        }

        /// <summary>
        /// Force content organizer timer job to run
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        public void ForceContentOrganizerTimerJobProcessing(SPSite site)
        {
             var contentOrganizerTimerJobGuid = new Guid("d6399a8e-b423-4833-b9b2-8fbe87ddd86d");

             var contentOrganizerJob = site.WebApplication.JobDefinitions[contentOrganizerTimerJobGuid];
            
            if (contentOrganizerJob != null)
            {
                // Mandatory to run with elevated privileges (Acces to SharePoint_config DB)
                SPSecurity.RunWithElevatedPrivileges(contentOrganizerJob.RunNow);              
            }
        }
    }
}
