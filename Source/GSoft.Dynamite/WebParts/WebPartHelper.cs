using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using GSoft.Dynamite.Fields.Constants;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Serializers;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;

namespace GSoft.Dynamite.WebParts
{
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
        /// Method to generate the html code to embed a web part in a Publishing Page Content,
        /// when you don't have access to the WebPart object. This will create a web part with
        /// all its properties set to default values.
        /// This method adds the web part to the "wpz" web part zone.
        /// </summary>
        /// <param name="web">the SPWeb</param>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPartName">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        public string GenerateWebPartHtmlByName(SPWeb web, SPListItem item, string webPartName)
        {
            Guid storageKey = this.EnsureWebPartByName(web, item, webPartName, "wpz", 0);
            string richContentEmbed = @"<div class='ms-rtestate-read ms-rte-wpbox'>
                      <div class='ms-rtestate-notify ms-rtestate-read {0}' id='div_{0}'></div>
                      <div id='vid_{0}' style='display:none'></div>
                  </div>";

            return string.Format(CultureInfo.InvariantCulture, richContentEmbed, storageKey.ToString());
        }

        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// when you can create the WebPart object yourself.
        /// This method adds the web part to the "wpz" web part zone.
        /// </summary>
        /// <param name="item">The item of the page to set the web part to</param>
        /// <param name="webPart">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Called method uses ListItem.")]
        public string GenerateWebPartHtml(SPListItem item, System.Web.UI.WebControls.WebParts.WebPart webPart)
        {
            Guid storageKey = this.EnsureWebPart(item, new WebPartInfo("wpz", webPart, 0));
            string richContentEmbed = "<div class=\"ms-rtestate-read ms-rte-wpbox\" contenteditable=\"false\">" +
                      "<div class=\"ms-rtestate-notify ms-rtestate-read {0}\" id=\"div_{0}\"></div>" +
                      "<div id=\"vid_{0}\" style=\"display:none\"></div>" +
                  "</div>";

            return string.Format(CultureInfo.InvariantCulture, richContentEmbed, storageKey.ToString());
        }

        /// <summary>
        /// Method to add a Web Part to a Web Part Zone when you don't have access to 
        /// the WebPart object. This will create a web part with all its properties set
        /// to default values as defined in the Web Part gallery.
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="item">The item of the page to add the web part to</param>
        /// <param name="webPartName">The filename of web part name to instanciate (name of the file in the Web Part gallery)</param>
        /// <param name="webPartZoneName">the web part zone to add the web part to</param>
        /// <param name="webPartZoneIndex">the web part zone index for ordering. (first = 0)</param>
        /// <returns>Return the Storage key of the web part</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Guid EnsureWebPartByName(SPWeb web, SPListItem item, string webPartName, string webPartZoneName, int webPartZoneIndex)
        {
            Guid storageKey = Guid.Empty;

            using (var manager = item.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
            {
                var webPart = this.CreateWebPartByFileName(web, webPartName, manager);

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
        /// Method to add a Web Part to a Web Part Zone when you have the fully-constructed WebPart
        /// instance available
        /// </summary>
        /// <param name="item">The item of the page to add the web part to</param>
        /// <param name="webPartInfo">The web part instance and its zone metadata</param>
        /// <returns>Return the Storage key of the web part</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        public Guid EnsureWebPart(SPListItem item, WebPartInfo webPartInfo)
        {
            Guid storageKey = Guid.Empty;

            using (var manager = item.File.GetLimitedWebPartManager(System.Web.UI.WebControls.WebParts.PersonalizationScope.Shared))
            {
                if (webPartInfo.WebPart != null)
                {
                    if (manager.WebParts.Cast<System.Web.UI.WebControls.WebParts.WebPart>()
                        .All(wp => wp.Title != webPartInfo.WebPart.Title))
                    {
                        manager.AddWebPart(webPartInfo.WebPart, webPartInfo.ZoneName, webPartInfo.ZoneIndex);
                        storageKey = manager.GetStorageKey(webPartInfo.WebPart);
                    }
                    else
                    {
                        this.logger.Warn("A WebPart with the name {0} already exists on the page {1}", webPartInfo.WebPart.Title, item[BuiltInFields.TitleName]);
                    }
                }
            }

            return storageKey;
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y)
        {
            return this.CreatePlaceholderWebPart(x, y, string.Empty, string.Empty, string.Empty);
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
            return this.CreatePlaceholderWebPart(x, y, backgroundColor, string.Empty, string.Empty);
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <param name="fontColor">Font hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <returns>
        /// A ContentEditorWebPart containing a PlaceHolder image
        /// </returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor, string fontColor)
        {
            return this.CreatePlaceholderWebPart(x, y, backgroundColor, fontColor, string.Empty);
        }

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <param name="fontColor">Font hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <param name="text">Custom text to show instead of the resolution.</param>
        /// <returns>
        /// A ContentEditorWebPart containing a PlaceHolder image
        /// </returns>
        public ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor, string fontColor, string text)
        {
            var textQueryString = string.IsNullOrEmpty(text) ? string.Empty : string.Format(CultureInfo.InvariantCulture, "&text={0}", HttpUtility.UrlEncode(text));
            var fontColorSlug = !string.IsNullOrEmpty(backgroundColor) && !string.IsNullOrEmpty(fontColor) ? string.Format(CultureInfo.InvariantCulture, "/{0}", fontColor) : string.Empty;
            var formattedContent = string.Format(
                CultureInfo.InvariantCulture, 
                "<img src=\"http://placehold.it/{0}x{1}/{2}{3}{4}\"/>", 
                x, 
                y, 
                backgroundColor, 
                fontColorSlug, 
                textQueryString);

            return new ContentEditorWebPart
            {
                Title = !string.IsNullOrEmpty(text) ? text : GenerateRandomPlaceholderTitle("Placeholder"),
                ChromeType = PartChromeType.None,
                Content = this.xmlHelper.CreateXmlElementInnerTextFromString(formattedContent)
            };
        }

        /// <summary>
        /// Method to create a web part with its default properties by matching with its
        /// file name from the Web Part gallery at the root of the site collection.
        /// </summary>
        /// <param name="web">The SPWeb where to create the web part</param>
        /// <param name="webPartName">The name of the web part to add</param>
        /// <param name="manager">The Web Part manager</param>
        /// <returns>The web part object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        private System.Web.UI.WebControls.WebParts.WebPart CreateWebPartByFileName(SPWeb web, string webPartName, SPLimitedWebPartManager manager)
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

        private static string GenerateRandomPlaceholderTitle(string prefix)
        {
            var random = new Random();
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", prefix, random.Next(1, 100));
        }
    }
}
