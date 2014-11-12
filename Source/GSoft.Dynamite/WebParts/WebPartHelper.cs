using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Serializers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;

namespace GSoft.Dynamite.WebParts
{
    using System.Web.UI.WebControls.WebParts;
    using GSoft.Dynamite.Fields.Constants;

    /// <summary>
    /// Class to manage WebParts, add WebPart to WebPartZone and other stuff
    /// </summary>
    public class WebPartHelper : IWebPartHelper
    {
        private readonly IXmlHelper xmlHelper;
        private readonly ILogger logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="xmlHelper">Helper for Xml work</param>
        /// <param name="logger">Logging utility</param>
        public WebPartHelper(IXmlHelper xmlHelper, ILogger logger)
        {
            this.xmlHelper = xmlHelper;
            this.logger = logger;
        }

        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// </summary>
        /// <param name="web">the SPWeb</param>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPartName">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        public string GenerateWebPartHtml(SPWeb web, SPListItem item, string webPartName)
        {
            Guid storageKey = this.AddWebPartToZone(web, item, webPartName, "wpz", 0);
            string richContentEmbed = @"<div class='ms-rtestate-read ms-rte-wpbox'>
                      <div class='ms-rtestate-notify ms-rtestate-read {0}' id='div_{0}'></div>
                      <div id='vid_{0}' style='display:none'></div>
                  </div>";

            return string.Format(CultureInfo.InvariantCulture, richContentEmbed, storageKey.ToString());
        }

        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// </summary>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPart">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Called method uses ListItem.")]
        public string GenerateWebPartHtml(SPListItem item, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            Guid storageKey = this.EnsureWebPartToZone(item, webPart, "wpz", 0);
            string richContentEmbed = "<div class=\"ms-rtestate-read ms-rte-wpbox\" contenteditable=\"false\">" +
                      "<div class=\"ms-rtestate-notify ms-rtestate-read {0}\" id=\"div_{0}\"></div>" +
                      "<div id=\"vid_{0}\" style=\"display:none\"></div>" +
                  "</div>";

            return string.Format(CultureInfo.InvariantCulture, richContentEmbed, storageKey.ToString());
        }

        /// <summary>
        /// Method to add a Web Part to a Web Part Zone
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="item">the item to add the web part to</param>
        /// <param name="webPartName">The web part name to get</param>
        /// <param name="webPartZoneName">the web part zone to add the web part to</param>
        /// <param name="webPartZoneIndex">the web part zone index for ordering. (first = 0)</param>
        /// <returns>Return the Storage key of the web part</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Guid AddWebPartToZone(SPWeb web, SPListItem item, string webPartName, string webPartZoneName, int webPartZoneIndex)
        {
            Guid storageKey = Guid.Empty;

            using (var manager = item.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
            {
                var webPart = this.CreateWebPart(web, webPartName, manager);

                if (webPart != null)
                {
                    webPart.ChromeType = System.Web.UI.WebControls.WebParts.PartChromeType.None;
                    manager.AddWebPart(webPart, webPartZoneName, webPartZoneIndex);
                    storageKey = manager.GetStorageKey(webPart);
                    webPart.Dispose();
                }
            }

            return storageKey;
        }

        /// <summary>
        /// Method to add a Web Part to a Web Part Zone
        /// </summary>
        /// <param name="item">the item to add the web part to</param>
        /// <param name="webPart">The web part name to get</param>
        /// <param name="webPartZoneName">the web part zone to add the web part to</param>
        /// <param name="webPartZoneIndex">the web part zone index for ordering. (first = 0)</param>
        /// <returns>Return the Storage key of the web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Keeping this signature for backwards compat with iO.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Guid EnsureWebPartToZone(SPListItem item, WebPart webPart, string webPartZoneName, int webPartZoneIndex)
        {
            Guid storageKey = Guid.Empty;

            using (var manager = item.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
            {
                if (webPart != null)
                {
                    if (manager.WebParts.Cast<WebPart>().All(wp => wp.Title != webPart.Title))
                    {
                        manager.AddWebPart(webPart, webPartZoneName, webPartZoneIndex);
                        storageKey = manager.GetStorageKey(webPart);
                    }
                    else
                    {
                        this.logger.Warn("A WebPart with the name {0} already exists on the page {1}", webPart.Title, item[BuiltInFields.TitleName]);
                    }
                }
            }

            return storageKey;
        }

        /// <summary>
        /// Method to create a web part.
        /// </summary>
        /// <param name="web">The SPWeb where to create the web part</param>
        /// <param name="webPartName">The name of the web part to add</param>
        /// <param name="manager">The Web Part manager</param>
        /// <returns>The web part object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public System.Web.UI.WebControls.WebParts.WebPart CreateWebPart(SPWeb web, string webPartName, SPLimitedWebPartManager manager)
        {
            SPQuery query = new SPQuery();
            query.Query = string.Format(CultureInfo.InvariantCulture, "<Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='File'>{0}</Value></Eq></Where>", webPartName);

            SPList webPartGallery = web.Site.RootWeb.GetCatalog(SPListTemplateType.WebPartCatalog);

            SPListItemCollection webParts = webPartGallery.GetItems(query);

            System.Web.UI.WebControls.WebParts.WebPart webPart = null;
            if (webParts.Count > 0)
            {
                XmlReader xmlReader = new XmlTextReader(webParts[0].File.OpenBinaryStream());
                string errorMessage;
                webPart = manager.ImportWebPart(xmlReader, out errorMessage);
            }

            return webPart;
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y)
        {
            return this.CreatePlaceholderWebPart(x, y, string.Empty, string.Empty);
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor)
        {
            return this.CreatePlaceholderWebPart(x, y, backgroundColor, string.Empty);
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <param name="fontColor">Font hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor, string fontColor)
        {
            var placeHolderWebPart = new ContentEditorWebPart();
            placeHolderWebPart.ChromeType = System.Web.UI.WebControls.WebParts.PartChromeType.None;

            string fontColorSlug = !string.IsNullOrEmpty(backgroundColor) && !string.IsNullOrEmpty(fontColor) ? string.Format(CultureInfo.InvariantCulture, "/{0}", fontColor) : string.Empty;
            string formattedContent = string.Format(CultureInfo.InvariantCulture, "<img src=\"http://placehold.it/{0}x{1}/{2}{3}\"/>", x, y, backgroundColor, fontColorSlug);

            placeHolderWebPart.Content = this.xmlHelper.CreateXmlElementInnerTextFromString(formattedContent);

            return placeHolderWebPart;
        }
    }
}
