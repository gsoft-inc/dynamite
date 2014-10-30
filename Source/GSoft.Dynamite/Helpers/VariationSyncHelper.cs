using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Globalization.Variations;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Helpers
{
    public class VariationSyncHelper : IVariationSyncHelper
    {
        private readonly ILogger _logger;
        private readonly IListHelper _listHelper;
        private readonly IVariationHelper _variationHelper;
        private const string PublishingAssemblyPath = @"C:\Program Files\Common Files\Microsoft Shared\Web Server Extensions\15\ISAPI\Microsoft.SharePoint.Publishing.dll";

        public VariationSyncHelper(ILogger logger, IVariationHelper variationHelper, IListHelper listHelper)
        {
            this._logger = logger;
            this._listHelper = listHelper;
            this._variationHelper = variationHelper;
        }

        /// <summary>
        /// Sync a SPList for multiple target labels
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="listInfo"></param>
        /// <param name="labels"></param>
        public void SyncList(SPWeb web, ListInfo listInfo, IList<VariationLabelInfo> labels)
        {
            var list = this._listHelper.EnsureList(web, listInfo);

            foreach (VariationLabelInfo label in labels)
            {
                // Synchronize only target labels
                if (!label.IsSource)
                {
                    this.SyncList(list, label.Title);
                }
            }
        }

        /// <summary>
        /// Sync a SPList for a target label
        /// </summary>
        /// <param name="listToSync">The source SPList instance to sync.</param>
        /// <param name="labelToSync">The label name to Sync. example: <c>"en"</c> or <c>"fr"</c>.</param>
        public void SyncList(SPList listToSync, string labelToSync)
        {
            this._logger.Info("Start method 'SyncList' for list: '{0}' with label '{1}'", listToSync.Title, labelToSync);

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

                        var publishingAssembly = Assembly.LoadFrom(PublishingAssemblyPath);
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
        /// Sync a SPList for multiple target labels
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="labels">Variations labels</param>
        public void SyncWeb(SPWeb web, IList<VariationLabelInfo> labels)
        {
            foreach (VariationLabelInfo label in labels)
            {
                // Synchronize only target labels
                if (!label.IsSource)
                {
                    this.SyncWeb(web, label.Title);
                }
            }
        }

        /// <summary>
        /// Sync a SPWeb with variations
        /// </summary>
        /// <param name="web">The source web instance to sync.</param>
        /// <param name="labelToSync">Source label to sync</param>
        public void SyncWeb(SPWeb web, string labelToSync)
        {
            this._logger.Info("Start method 'SyncWeb' for web: '{0}' with label '{1}'", web.Url, labelToSync);

            var publishingAssembly = Assembly.LoadFrom(PublishingAssemblyPath);
            var workItemHelper = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.VariationWorkItemHelper");
            var cachedVariationSettings = publishingAssembly.GetType("Microsoft.SharePoint.Publishing.Internal.CachedVariationSettings");

            // Get the labels to sync
            var labels = this._variationHelper.GetVariationLabels(web.Site, labelToSync);

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
    }
}
