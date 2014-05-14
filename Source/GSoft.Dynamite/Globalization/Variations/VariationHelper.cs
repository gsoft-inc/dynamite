using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// Helper from a old codebase. Must be merge with Expert and/or Builder
    /// We use the 2 sync method in the PowerShell assembly.
    /// </summary>
    [Obsolete]
    public class VariationHelper
    {
        private readonly ILogger logger;
        private readonly string publishingAssemblyPath = @"C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Publishing.dll";

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        public VariationHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Determines whether [the specified web] [is current web source label].
        /// </summary>
        /// <param name="web">The web.</param>
        /// <returns>A boolean value which indicates if the current web is the source variation label.</returns>
        public bool IsCurrentWebSourceLabel(SPWeb web)
        {
            var sourceLabel = Microsoft.SharePoint.Publishing.Variations.GetLabels(web.Site).FirstOrDefault(x => x.IsSource);
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
        /// <param name="site">The site.</param>
        /// <param name="labelToSync">The label name to Sync. eg. "en" or "fr".</param>
        /// <returns>A collection of unique label.</returns>
        public ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel> GetVariationLabels(SPSite site, string labelToSync)
        {
            this.logger.Info("Start method 'GetVariationLabels' for site url: '{0}' with label '{1}'", site.Url, labelToSync);

            var web = site.RootWeb;
            var variationLabelsList = web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, "/Variation Labels/Allitems.aspx"));
            var list = new List<Microsoft.SharePoint.Publishing.VariationLabel>();
            var query = new SPQuery
            {
                Query = "<Where><Eq><FieldRef Name='Title'/><Value Type='Text'>" + labelToSync + "</Value></Eq></Where><OrderBy><FieldRef Name=\"Title\" Ascending=\"TRUE\"></FieldRef></OrderBy>"
            };

            foreach (SPListItem item in variationLabelsList.GetItems(query))
            {
                var webUrl = (string)item["Top Web URL"];
                webUrl = webUrl.Substring(webUrl.IndexOf(',') + 1);

                var pubWeb = PublishingWeb.GetPublishingWeb(site.OpenWeb(webUrl));
                var varLbl = pubWeb.Label;
                list.Add(varLbl);
            }

            return new ReadOnlyCollection<Microsoft.SharePoint.Publishing.VariationLabel>(list);
        }

        /// <summary>
        /// Sync a SPList for a target label
        /// </summary>
        /// <param name="listToSync">The source SPList instance to sync.</param>
        /// <param name="labelToSync">The label name to Sync. eg. "en" or "fr".</param>
        public void SyncList(SPList listToSync, string labelToSync)
        {
            this.logger.Info("Start method 'SyncList' for list: '{0}' with label '{1}'", listToSync.Title, labelToSync);

            var sourceWeb = listToSync.ParentWeb;
            Guid sourceListGuid = listToSync.ID;

            /* Note: This code is dessigned for work with internals SharePoint methods implemented in the Microsoft.SharePoint.Publishing assembly
             * See "PerformListLabelAction" method in "CreateVariationsPage" (Microsoft.SharePoint.Publishing.Internal.CodeBehind) 
             * See "EnqueueWorkItemsForList" method in "VariationWorkItemHelper" (Microsoft.SharePoint.Publishing.Internal)
             */
            SPSecurity.RunWithElevatedPrivileges((SPSecurity.CodeToRunElevated)(() =>
            {
                using (var elevatedSite = new SPSite(sourceWeb.Site.ID))
                {
                    using (var elevatedWeb = elevatedSite.OpenWeb(sourceWeb.ID))
                    {
                        var list = elevatedWeb.Lists[sourceListGuid];

                        var publishingAssembly = Assembly.LoadFrom(this.publishingAssemblyPath);
                        var workItemHelper = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.VariationWorkItemHelper");
                        var multiLingualResourceList = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.MultiLingualResourceList");

                        var types = new Type[1];
                        types[0] = typeof(SPList);

                        var args = new object[1];
                        args[0] = list;

                        var resParam = new object[2];
                        resParam[0] = list;
                        resParam[1] = true;

                        // Initialize the list
                        var nominateResources = multiLingualResourceList.GetMethod("NominateResource", BindingFlags.Public | BindingFlags.Static);
                        nominateResources.Invoke(null, resParam);

                        var ctor = multiLingualResourceList.GetConstructor(types);
                        var multilingualList = ctor.Invoke(args);

                        // Very important to set the HttpContext to null (AllowUnsafeUpdates is ignored by the SharePoint method)
                        HttpContext.Current = null;

                        var workItemParam = new object[3];
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
        /// Sync a SPWeb with variations
        /// </summary>
        /// <param name="web">The source web instance to sync.</param>
        /// <param name="labelToSync">Source label to sync</param>
        public void SyncWeb(SPWeb web, string labelToSync)
        {
            this.logger.Info("Start method 'SyncWeb' for web: '{0}' with label '{1}'", web.Url, labelToSync);

            var publishingAssembly = Assembly.LoadFrom(this.publishingAssemblyPath);
            var workItemHelper = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.VariationWorkItemHelper");
            var cachedVariationSettings = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.CachedVariationSettings");

            // Get the labels to sync
            var labels = this.GetVariationLabels(web.Site, labelToSync);

            // Very important to set the HttpContext to null (AllowUnsafeUpdates is ignored by the SharePoint method)
            HttpContext.Current = null;

            Type[] methodParam = new Type[3];
            methodParam[0] = typeof(SPSite);
            methodParam[1] = typeof(SPUrlZone);
            methodParam[2] = typeof(bool);

            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

            object[] cvsParam = new object[3];
            cvsParam[0] = web.Site;
            cvsParam[1] = web.Site.Zone;
            cvsParam[2] = true;

            var cvsMethod = cachedVariationSettings.GetMethod("CreateVariationSettings", bindingFlags, null, methodParam, null);
            var cvs = cvsMethod.Invoke(null, cvsParam);

            object[] workItemParam = new object[3];
            workItemParam[0] = web;
            workItemParam[1] = labels;
            workItemParam[2] = cvs;

            var method = workItemHelper.GetMethod("EnqueueCreateSiteJobs", bindingFlags);
            method.Invoke(null, workItemParam);
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
