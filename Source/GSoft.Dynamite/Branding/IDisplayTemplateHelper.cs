using System.Collections.Generic;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// Utility to manage display templates
    /// </summary>
    public interface IDisplayTemplateHelper
    {
        /// <summary>
        /// Folder path to display template
        /// </summary>
        string DisplayTemplatesFolder { get; }

        /// <summary>
        /// Subfolder path to content search web part display templates
        /// </summary>
        string ContentWebPartFolder { get; }

        /// <summary>
        /// Subfolder path to search result display templates
        /// </summary>
        string SearchFolder { get; }

        /// <summary>
        /// Subfolder path to filters display templates
        /// </summary>
        string FilterFolder { get; }

        /// <summary>
        /// Generates the java script file corresponding to the HTML file.
        /// </summary>
        /// <param name="htmlFiles">The HTML files.</param>
        void GenerateJavaScriptFile(IList<SPFile> htmlFiles);
    }
}