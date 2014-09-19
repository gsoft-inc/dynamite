using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Xml;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Setup
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.Office.RecordsManagement.PolicyFeatures;

    /// <summary>
    /// List view builder.
    /// </summary>
    public class ViewBuilder
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewBuilder"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ViewBuilder(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Ensures the view.
        /// </summary>
        /// <param name="viewCollection">The view collection.</param>
        /// <param name="viewInfo">The view information.</param>
        /// <returns>The ensured view.</returns>
        public SPView EnsureView(SPViewCollection viewCollection, ViewInfo viewInfo)
        {
            var views = viewCollection.Cast<SPView>();
            var ensuredView = views.SingleOrDefault(view => view.Title.Equals(viewInfo.Name, StringComparison.OrdinalIgnoreCase));

            // If view collection doesn't already contain a view with the same name, create it
            if (ensuredView == null)
            {
                // Create view fields string collection
                var viewFields = new StringCollection();
                viewFields.AddRange(viewInfo.ViewFields);

                if (!string.IsNullOrEmpty(viewInfo.ProjectedFields) || !string.IsNullOrEmpty(viewInfo.Joins))
                {
                    ensuredView = viewCollection.Add(
                        viewInfo.Name,
                        viewFields,
                        viewInfo.Query,
                        viewInfo.Joins,
                        viewInfo.ProjectedFields,
                        viewInfo.RowLimit,
                        viewInfo.IsPaged,
                        viewInfo.IsDefaultView,
                        viewInfo.ViewType,
                        viewInfo.IsPersonalView);
                }
                else
                {
                    ensuredView = viewCollection.Add(
                        viewInfo.Name,
                        viewFields,
                        viewInfo.Query,
                        viewInfo.RowLimit,
                        viewInfo.IsPaged,
                        viewInfo.IsDefaultView,
                        viewInfo.ViewType,
                        viewInfo.IsPersonalView); 
                }

                viewCollection.List.Update();
                this.logger.Info(
                    "View '{0}' has been successfully created in list '{1}'.",
                    viewInfo.Name,
                    viewCollection.List.Title);
            }
            else
            {
                this.logger.Warn(
                    "View '{0}' has already been created in list '{1}'.  Updating the view.", 
                    viewInfo.Name, 
                    viewCollection.List.Title);

                // Update the existing view
                ensuredView.ViewFields.DeleteAll();
                viewInfo.ViewFields.ToList().ForEach(vf => ensuredView.ViewFields.Add(vf));
                ensuredView.Query = viewInfo.Query;
                ensuredView.Joins = viewInfo.Joins;
                ensuredView.ProjectedFields = viewInfo.ProjectedFields;
                ensuredView.RowLimit = viewInfo.RowLimit;
                ensuredView.Paged = viewInfo.IsPaged;
                ensuredView.DefaultView = viewInfo.IsDefaultView;
            }

            if (!string.IsNullOrEmpty(viewInfo.ViewData))
            {
                ensuredView.ViewData = viewInfo.ViewData; 
            }

            ensuredView.Update();

            return ensuredView;
        }

        /// <summary>
        /// Ensures the calendar overlays.
        /// Note: This clears the existing overlays.
        /// </summary>
        /// <param name="viewCollection">The view collection.</param>
        /// <param name="overlayInfos">The overlay information objects.</param>
        public void EnsureCalendarOverlays(SPViewCollection viewCollection, CalendarOverlayInfo[] overlayInfos)
        {
            for (var i = 0; i < overlayInfos.Length; i++)
            {
                var overlayInfo = overlayInfos[i];
                AddCalendarOverlay(
                    viewCollection.List,
                    overlayInfo.TargetViewName,
                    overlayInfo.OverlayViewName,
                    null,
                    null,
                    viewCollection.List,
                    overlayInfo.Name,
                    overlayInfo.Description,
                    overlayInfo.Color,
                    overlayInfo.AlwaysShow,
                    i == 0);
            }
        }

        /// <summary>
        /// Ensures the calendar overlays.
        /// Note: This clears the existing overlays.
        /// </summary>
        /// <param name="viewCollection">The view collection.</param>
        /// <param name="overlayInfos">The overlay information objects.</param>
        /// <param name="overlayList">The overlay list.</param>
        public void EnsureCalendarOverlays(SPViewCollection viewCollection, CalendarOverlayInfo[] overlayInfos, SPList overlayList)
        {
            for (var i = 0; i < overlayInfos.Length; i++)
            {
                var overlayInfo = overlayInfos[i];
                AddCalendarOverlay(
                    overlayList,
                    overlayInfo.TargetViewName,
                    overlayInfo.OverlayViewName,
                    null,
                    null,
                    overlayList,
                    overlayInfo.Name,
                    overlayInfo.Description,
                    overlayInfo.Color,
                    overlayInfo.AlwaysShow,
                    i == 0);
            }
        }

        /// <summary>
        /// Adds the calendar overlay.
        /// Please spare me with the code quality...
        /// Code taken from Gary Lapointe's blog: http://blog.falchionconsulting.com/index.php/2011/06/programmatically-setting-sharepoint-2010-calendar-overlays/
        /// </summary>
        /// <param name="targetList">The target list.</param>
        /// <param name="viewName">Name of the view.</param>
        /// <param name="overlayViewName">Name of the overlay view.</param>
        /// <param name="owaUrl">The Office Web Access URL.</param>
        /// <param name="exchangeUrl">The exchange URL.</param>
        /// <param name="overlayList">The overlay list.</param>
        /// <param name="overlayName">Name of the overlay.</param>
        /// <param name="overlayDescription">The overlay description.</param>
        /// <param name="color">The color.</param>
        /// <param name="alwaysShow">if set to <c>true</c> [always show].</param>
        /// <param name="clearExisting">if set to <c>true</c> [clear existing].</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private static void AddCalendarOverlay(SPList targetList, string viewName, string overlayViewName, string owaUrl, string exchangeUrl, SPList overlayList, string overlayName, string overlayDescription, CalendarOverlayColor color, bool alwaysShow, bool clearExisting)
        {
            var isSharePointOverlay = overlayList != null;
            var overlayViewId = string.IsNullOrEmpty(overlayViewName) ? overlayList.DefaultView.ID : overlayList.Views[overlayViewName].ID;
            var linkUrl = isSharePointOverlay ? overlayList.GetView(overlayViewId).Url : owaUrl;

            var targetView = targetList.DefaultView;
            if (!string.IsNullOrEmpty(viewName))
            {
                targetView = targetList.Views[viewName];
            }

            var xml = new XmlDocument();
            XmlElement aggregationElement = null;
            var count = 0;
            if (string.IsNullOrEmpty(targetView.CalendarSettings) || clearExisting)
            {
                xml.AppendChild(xml.CreateElement("AggregationCalendars"));
                aggregationElement = xml.CreateElement("AggregationCalendar");
                if (xml.DocumentElement != null)
                {
                    xml.DocumentElement.AppendChild(aggregationElement);
                }
            }
            else
            {
                xml.LoadXml(targetView.CalendarSettings);
                var calendars = xml.SelectNodes("/AggregationCalendars/AggregationCalendar");
                if (calendars != null)
                {
                    count = calendars.Count;
                }

                aggregationElement =
                    xml.SelectSingleNode(
                        string.Format("/AggregationCalendars/AggregationCalendar[@CalendarUrl='{0}']", linkUrl)) as XmlElement;

                if (aggregationElement == null)
                {
                    if (count >= 10)
                    {
                        throw new SPException(
                            string.Format(
                                "10 calendar ovarlays already exist for the calendar {0}.",
                                targetList.RootFolder.ServerRelativeUrl));
                    }

                    aggregationElement = xml.CreateElement("AggregationCalendar");
                    if (xml.DocumentElement != null)
                    {
                        xml.DocumentElement.AppendChild(aggregationElement);
                    }
                }
            }

            if (!aggregationElement.HasAttribute("Id"))
            {
                aggregationElement.SetAttribute("Id", Guid.NewGuid().ToString("B", CultureInfo.InvariantCulture));
            }

            aggregationElement.SetAttribute("Type", isSharePointOverlay ? "SharePoint" : "Exchange");
            aggregationElement.SetAttribute("Name", !string.IsNullOrEmpty(overlayName) ? overlayName : (overlayList == null ? string.Empty : overlayList.Title));
            aggregationElement.SetAttribute("Description", !string.IsNullOrEmpty(overlayDescription) ? overlayDescription : (overlayList == null ? string.Empty : overlayList.Description));
            aggregationElement.SetAttribute("Color", ((int)color).ToString(CultureInfo.InvariantCulture));
            aggregationElement.SetAttribute("AlwaysShow", alwaysShow.ToString());
            aggregationElement.SetAttribute("CalendarUrl", linkUrl);

            var settingsElement = aggregationElement.SelectSingleNode("./Settings") as XmlElement;
            if (settingsElement == null)
            {
                settingsElement = xml.CreateElement("Settings");
                aggregationElement.AppendChild(settingsElement);
            }

            if (isSharePointOverlay)
            {
                settingsElement.SetAttribute("WebUrl", overlayList.ParentWeb.Site.MakeFullUrl(overlayList.ParentWebUrl));
                settingsElement.SetAttribute("ListId", overlayList.ID.ToString("B", CultureInfo.InvariantCulture));
                settingsElement.SetAttribute("ViewId", overlayViewId.ToString("B", CultureInfo.InvariantCulture));
                settingsElement.SetAttribute("ListFormUrl", overlayList.Forms[PAGETYPE.PAGE_DISPLAYFORM].ServerRelativeUrl);
            }
            else
            {
                settingsElement.SetAttribute("ServiceUrl", exchangeUrl);
            }

            targetView.CalendarSettings = xml.OuterXml;
            targetView.Update();
        }
    }
}
