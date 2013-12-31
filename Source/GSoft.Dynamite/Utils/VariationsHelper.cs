using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Web;
using System.Collections.Generic;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Variations helper class.
    /// </summary>
    public class VariationsHelper
    {
        /// <summary>
        /// Determines whether [the specified web] [is current web source label].
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns>A boolean value which indicates if the current web is the source variation label.</returns>
        public bool IsCurrentWebSourceLabel(SPWeb web)
        {
            var sourceLabel = Variations.GetLabels(web.Site).FirstOrDefault(x => x.IsSource);
            if (sourceLabel != null)
            {
                // Compare absolute URL values
                return web.Url.StartsWith(sourceLabel.TopWebUrl, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Get the variations labels for the site collection.
        /// NOTE: Also possible with the static Microsoft.SharePoint.Publishing Variations object by faking a SPContext
        /// </summary>
        /// <param name="Site">The site.</param>
        /// <param name="labelToSync">The label name to Sync. eg. "en" or "fr".</param>
        /// <returns>A collection of unique label.</returns>
        public ReadOnlyCollection<VariationLabel> GetVariationLabels(SPSite Site, string labelToSync)
        {
            SPWeb spWeb = Site.RootWeb;
            SPList variationLabelsList = spWeb.GetList(spWeb.ServerRelativeUrl + "/Variation Labels/Allitems.aspx");
            List<VariationLabel> list = new List<VariationLabel>();
            SPQuery query = new SPQuery
            {
                Query = "<Where><Eq><FieldRef Name='Title'/><Value Type='Text'>" + labelToSync + "</Value></Eq></Where><OrderBy><FieldRef Name=\"Title\" Ascending=\"TRUE\"></FieldRef></OrderBy>"
            };

            foreach (SPListItem item in variationLabelsList.GetItems(query))
            {
                string webUrl = (string)item["Top Web URL"];
                webUrl = webUrl.Substring(webUrl.IndexOf(',') + 1);

                PublishingWeb pubWeb = PublishingWeb.GetPublishingWeb(Site.OpenWeb(webUrl));
                VariationLabel varLbl = pubWeb.Label;
                list.Add(varLbl);
            }

            return new ReadOnlyCollection<VariationLabel>((IList<VariationLabel>)list);
        }

        /// <summary>
        /// Sync a SPList for a target label
        /// </summary>
        /// <param name="listToSync">The source SPList instance to sync.</param>
        /// <param name="labelToSync">The label name to Sync. eg. "en" or "fr".</param>
        public void SyncList(SPList listToSync, string labelToSync)
        {
            var sourceWeb = listToSync.ParentWeb;
            Guid sourceListGuid = listToSync.ID;

            /* Note: This code is dessigned for work with internals SharePoint methods implemented in the Microsoft.SharePoint.Publishing assembly
             * See "PerformListLabelAction" method in "CreateVariationsPage" (Microsoft.SharePoint.Publishing.Internal.CodeBehind) 
             * See "EnqueueWorkItemsForList" method in "VariationWorkItemHelper" (Microsoft.SharePoint.Publishing.Internal)
             */
            SPSecurity.RunWithElevatedPrivileges((SPSecurity.CodeToRunElevated)(() =>
            {
                using (SPSite elevatedSite = new SPSite(sourceWeb.Site.ID))
                {
                    using (SPWeb elevatedWeb = elevatedSite.OpenWeb(sourceWeb.ID))
                    {
                        var list = elevatedWeb.Lists[sourceListGuid];

                        var publishingAssembly = Assembly.LoadFrom("C:\\Program Files\\Common Files\\Microsoft Shared\\Web Server Extensions\\15\\ISAPI\\Microsoft.SharePoint.Publishing.dll");
                        var workItemHelper = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.VariationWorkItemHelper");
                        Type MultiLingualResourceList = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.MultiLingualResourceList");

                        Type[] types = new Type[1];
                        types[0] = typeof(SPList);

                        Object[] args = new Object[1];
                        args[0] = list;

                        Object[] resParam = new Object[2];
                        resParam[0] = list;
                        resParam[1] = true;

                        // Initialize the list
                        var nominateResources = MultiLingualResourceList.GetMethod("NominateResource", BindingFlags.Public | BindingFlags.Static);
                        nominateResources.Invoke(null, resParam);

                        var ctor = MultiLingualResourceList.GetConstructor(types);
                        var multilingualList = ctor.Invoke(args);

                        // Get the labels to sync
                        var labels = this.GetVariationLabels(sourceWeb.Site, labelToSync);

                        // Very important to set the HttpComnterct to null (AllowUnsafeUpdates is ignored by the SharePoint method)
                        HttpContext.Current = null;

                        Object[] workItemParam = new Object[3];
                        workItemParam[0] = elevatedSite;
                        workItemParam[1] = elevatedWeb;
                        workItemParam[2] = multilingualList;

                        // Method "EnqueueCreateListWorkItem" process one label at time! 
                        var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
                        var method = workItemHelper.GetMethod("EnqueueWorkItemsForList", bindingFlags);
                        method.Invoke(null, workItemParam);
                    }
                }
            }));
        }

        /// <summary>
        /// Get the hidden relationships list for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>The relationships list.</returns>
        private SPList GetVariationLabelHiddenList(SPSite site)
        {
            var guid = new Guid(site.RootWeb.GetProperty("_VarLabelsListId").ToString());
            return site.RootWeb.Lists[guid];
        }

        /// <summary>
        /// Get the hidden variation labels for a site collection.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>the variation labels list.</returns>
        private SPList GetRelationshipsHiddenList(SPSite site)
        {
            var guid = new Guid(site.RootWeb.GetProperty("_VarRelationshipsListId").ToString());
            return site.RootWeb.Lists[guid];
        }
    }
}
