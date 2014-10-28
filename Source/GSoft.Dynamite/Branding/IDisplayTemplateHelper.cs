namespace GSoft.Dynamite.Branding
{
    using System.Collections.Generic;

    using Microsoft.SharePoint;

    public interface IDisplayTemplateHelper
    {
        /// <summary>
        /// Generates the java script file corresponding to the HTML file.
        /// </summary>
        /// <param name="htmlFiles">The HTML files.</param>
        void GenerateJavaScriptFile(IList<SPFile> htmlFiles);
    }
}