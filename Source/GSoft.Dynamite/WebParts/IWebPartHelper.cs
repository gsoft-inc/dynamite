namespace GSoft.Dynamite.WebParts
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.WebPartPages;

    /// <summary>
    /// Web Part helper
    /// </summary>
    public interface IWebPartHelper
    {
        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// </summary>
        /// <param name="web">the SPWeb</param>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPartName">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        string GenerateWebPartHtml(SPWeb web, SPListItem item, string webPartName);

        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// </summary>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPart">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Called method uses ListItem.")]
        string GenerateWebPartHtml(SPListItem item, System.Web.UI.WebControls.WebParts.WebPart webPart);

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
        Guid AddWebPartToZone(SPWeb web, SPListItem item, string webPartName, string webPartZoneName, int webPartZoneIndex);

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
        Guid EnsureWebPartToZone(SPListItem item, System.Web.UI.WebControls.WebParts.WebPart webPart, string webPartZoneName, int webPartZoneIndex);

        /// <summary>
        /// Method to create a web part.
        /// </summary>
        /// <param name="web">The SPWeb where to create the web part</param>
        /// <param name="webPartName">The name of the web part to add</param>
        /// <param name="manager">The Web Part manager</param>
        /// <returns>The web part object</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        System.Web.UI.WebControls.WebParts.WebPart CreateWebPart(SPWeb web, string webPartName, SPLimitedWebPartManager manager);

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        ContentEditorWebPart CreatePlaceholderWebPart(int x, int y);

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor);

        /// <summary>
        /// Method to create a Content Editor Web Part containing a place holder image
        /// </summary>
        /// <param name="x">x axis dimension in pixel</param>
        /// <param name="y">y axis dimension in pixel</param>
        /// <param name="backgroundColor">Background hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <param name="fontColor">Font hexadecimal color ex: <c>"ffffff"</c> or <c>"e3b489"</c></param>
        /// <returns>A ContentEditorWebPart containing a PlaceHolder image</returns>
        ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor, string fontColor);
    }
}