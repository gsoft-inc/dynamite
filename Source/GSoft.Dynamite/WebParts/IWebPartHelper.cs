using System.Collections.Generic;

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
        /// Method to generate the html code to embed a web part in a Publishing Page Content,
        /// when you don't have access to the WebPart object. This will create a web part with
        /// all its properties set to default values.
        /// This method adds the web part to the "wpz" web part zone.
        /// </summary>
        /// <param name="web">the SPWeb</param>
        /// <param name="item">The item to set the web part to</param>
        /// <param name="webPartName">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        string GenerateWebPartHtmlByName(SPWeb web, SPListItem item, string webPartName);

        /// <summary>
        /// Method to generate the html code to embed a web part in a Publishing Page Content
        /// when you can create the WebPart object yourself.
        /// This method adds the web part to the "wpz" web part zone.
        /// </summary>
        /// <param name="item">The item of the page to set the web part to</param>
        /// <param name="webPart">the name of the web part</param>
        /// <returns>The html code that embed a web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Called method uses ListItem.")]
        string GenerateWebPartHtml(SPListItem item, System.Web.UI.WebControls.WebParts.WebPart webPart);

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
        Guid EnsureWebPartByName(SPWeb web, SPListItem item, string webPartName, string webPartZoneName, int webPartZoneIndex);

        /// <summary>
        /// Method to add a Web Part to a Web Part Zone when you have the fully-constructed WebPart
        /// instance available
        /// </summary>
        /// <param name="item">The item of the page to add the web part to</param>
        /// <param name="webPartInfo">The web part instance and its zone metadata</param>
        /// <returns>Return the Storage key of the web part</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Keeping this signature for backwards compat with iO.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        Guid EnsureWebPart(SPListItem item, WebPartInfo webPartInfo);

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
        ContentEditorWebPart CreatePlaceholderWebPart(int x, int y, string backgroundColor, string fontColor, string text);

        /// <summary>
        /// Creates a responsive placeholder web part.
        /// </summary>
        /// <param name="height">The height of the placeholder in pixels.</param>
        /// <param name="backgroundColor">Color of the background (ex: #abc).</param>
        /// <param name="fontColor">Color of the font (ex: #fff).</param>
        /// <param name="text">The placeholder text.</param>
        /// <returns>A content editor Web Part containing a responsive placeholder.</returns>
        ContentEditorWebPart CreateResponsivePlaceholderWebPart(
            int height,
            string backgroundColor,
            string fontColor,
            string text);

        /// <summary>
        /// Creates a responsive placeholder web part.
        /// </summary>
        /// <param name="height">The height of the placeholder in pixels.</param>
        /// <param name="backgroundColor">Color of the background (ex: #abc).</param>
        /// <param name="fontColor">Color of the font (ex: #fff).</param>
        /// <param name="text">The placeholder text.</param>
        /// <param name="extraCssClasses">Css classes to be added to the webpart.</param>
        /// <returns>A content editor Web Part containing a responsive placeholder.</returns>
        ContentEditorWebPart CreateResponsivePlaceholderWebPart(
            int height,
            string backgroundColor,
            string fontColor,
            string text,
            ICollection<string> extraCssClasses);
    }
}