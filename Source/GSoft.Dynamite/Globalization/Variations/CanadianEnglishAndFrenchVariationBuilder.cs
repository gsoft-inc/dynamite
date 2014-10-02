using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.TimerJobs;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Globalization.Variations
{
    /// <summary>
    /// A variation builder that creates one Canadian English-culture source variation label
    /// and its Canadian French-culture destination.
    /// </summary>
    [Obsolete("Use the VariationHelper class")]
    public class CanadianEnglishAndFrenchVariationBuilder : IVariationBuilder
    {
        private static IList<VariationLabelInfo> labels = new List<VariationLabelInfo>()
        {
            new VariationLabelInfo
                {
                    Title = "en", 
                    FlagControlDisplayName = "English",
                    Language = "en-US",
                    Locale = new CultureInfo("en-CA").LCID,     // 4105
                    HierarchyCreationMode = CreationMode.PublishingSitesAndAllPages,
                    IsSource = true
                },
            new VariationLabelInfo
                {
                    Title = "fr",
                    FlagControlDisplayName = "Français",
                    Language = "fr-FR",
                    Locale = new CultureInfo("fr-CA").LCID,     // 3084
                    HierarchyCreationMode = CreationMode.PublishingSitesAndAllPages
                }
        };

        private readonly ITimerJobExpert timerJobExpert;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanadianEnglishAndFrenchVariationBuilder"/> class.
        /// </summary>
        /// <param name="timerJobExpert">
        /// The timer job expert.
        /// </param>
        public CanadianEnglishAndFrenchVariationBuilder(ITimerJobExpert timerJobExpert)
        {
            this.timerJobExpert = timerJobExpert;
        }

        /// <summary>
        /// The available labels.
        /// </summary>
        public static IList<VariationLabelInfo> Labels
        {
            get
            {
                return labels;
            }
        }

        /// <summary>
        /// The configure variations settings method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        public void ConfigureVariationsSettings(SPSite site)
        {
            var rootWeb = site.RootWeb;
            Guid varRelationshipsListId = new Guid(rootWeb.AllProperties["_VarRelationshipsListId"] as string);
            SPList varRelationshipsList = rootWeb.Lists[varRelationshipsListId];
            SPFolder rootFolder = varRelationshipsList.RootFolder;

            // Automatic creation
            rootFolder.Properties["EnableAutoSpawnPropertyName"] = "true";

            // Recreate Deleted Target Page; set to false to enable recreation
            rootFolder.Properties["AutoSpawnStopAfterDeletePropertyName"] = "false";

            // Update Target Page Web Parts
            rootFolder.Properties["UpdateWebPartsPropertyName"] = "true";

            // Resources
            rootFolder.Properties["CopyResourcesPropertyName"] = "true";

            // Notification
            rootFolder.Properties["SendNotificationEmailPropertyName"] = "false";
            rootFolder.Properties["SourceVarRootWebTemplatePropertyName"] = "CMSPUBLISHING#0";
            rootFolder.Update();

            SPListItem item = null;
            if (varRelationshipsList.Items.Count > 0)
            {
                item = varRelationshipsList.Items[0];
            }
            else
            {
                item = varRelationshipsList.Items.Add();
                item["GroupGuid"] = new Guid("F68A02C8-2DCC-4894-B67D-BBAED5A066F9");
            }

            item["Deleted"] = false;
            item["ObjectID"] = rootWeb.ServerRelativeUrl;
            item["ParentAreaID"] = string.Empty;

            item.Update();
        }

        /// <summary>
        /// The create variations method.
        /// </summary>
        /// <param name="site">
        /// The site collection.
        /// </param>
        public void CreateVariations(SPSite site)
        {
            var rootWeb = site.RootWeb;
            Guid varListId = new Guid(rootWeb.AllProperties["_VarLabelsListId"] as string);
            SPList varList = rootWeb.Lists[varListId];

            foreach (VariationLabelInfo label in Labels)
            {
                SPListItem item;
                var existingItems = varList.Items.Cast<SPListItem>().Where(listItem => listItem.Title == label.Title).ToList();

                if (existingItems.Count > 0)
                {
                    item = existingItems[0];
                }
                else
                {
                    // create the label
                    item = varList.Items.Add();
                }
                 
                item[SPBuiltInFieldId.Title] = label.Title;
                item["Description"] = label.Description;
                item["Flag Control Display Name"] = label.FlagControlDisplayName;
                item["Language"] = label.Language;
                item["Locale"] = label.Locale.ToString(CultureInfo.InvariantCulture);
                item["Hierarchy Creation Mode"] = label.HierarchyCreationMode;
                item["Is Source"] = label.IsSource.ToString();

                if (existingItems.Count > 0)
                {
                    // assume hierarchy already exists also
                    item["Hierarchy Is Created"] = true;
                }
                else 
                {
                    item["Hierarchy Is Created"] = false;
                }

                item.Update();
            }
        }

        /// <summary>
        /// The create hierarchies.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        public void CreateHierarchies(SPSite site)
        {
            this.timerJobExpert.CreateJob(site, new Guid("e7496be8-22a8-45bf-843a-d1bd83aceb25"));

            var jobId = this.timerJobExpert.StartJob(site, "VariationsCreateHierarchies");
            
            DateTime startTime = DateTime.Now.ToUniversalTime();

            this.timerJobExpert.WaitForJob(site, jobId, startTime);

            // Force the title of the label subsites, because the value of Flag Control Display Name doesn't get respected on destination labels most of the time.
            // Also take care of setting the regional settings on each site.
            foreach (VariationLabelInfo label in Labels)
            {
                using (var labelWeb = site.OpenWeb(label.Title))
                {
                    // UICulture's gotta be in the same locale as the web being renamed (otherwise change won't go through - thanks MUI!)
                    var previousUiCulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(label.Language);

                    labelWeb.Title = label.FlagControlDisplayName;
                    labelWeb.Update();

                    Thread.CurrentThread.CurrentUICulture = previousUiCulture;
                }
            }
        }
    }
}
