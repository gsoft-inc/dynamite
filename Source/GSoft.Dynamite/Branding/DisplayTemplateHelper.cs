using System;
using System.Collections.Generic;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// Helper class for display template work
    /// </summary>
    public class DisplayTemplateHelper : IDisplayTemplateHelper
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayTemplateHelper"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public DisplayTemplateHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Folder name for Display Templates
        /// </summary>
        public string DisplayTemplatesFolder 
        {
            get
            {
                return "Display Templates";
            }
        }

        /// <summary>
        /// Folder name for Content WebPart Folder
        /// </summary>
        public string ContentWebPartFolder
        {
            get 
            { 
                return "Content Web Parts"; 
            }
        }

        /// <summary>
        /// Folder name for Search 
        /// </summary>
        public string SearchFolder
        {
            get
            {
                return "Search";
            }
        }

        /// <summary>
        /// Folder name for Filter
        /// </summary>
        public string FilterFolder
        {
            get
            {
                return "Filters";         
            }
        }

        /// <summary>
        /// Generates the java script file corresponding to the HTML file.
        /// </summary>
        /// <param name="htmlFiles">The HTML files.</param>
        public void GenerateJavaScriptFile(IList<SPFile> htmlFiles)
        {
            foreach (var htmlFile in htmlFiles)
            {
                try
                {
                    // undo the custization, necessary only upon successive feature re-activations (because the Checkout and edits below cause the unghosting/customization of the file)
                    htmlFile.RevertContentStream(); 
                }
                catch (Exception exception)
                {
                    this.logger.Error("Failed to undo customization while re-provisioning Display Templates. Exception: {0} StackTrace: {1}", exception.Message, exception.StackTrace);
                }

                htmlFile.CheckOut();
                htmlFile.Update();
                htmlFile.CheckIn("Generate JS File");
                htmlFile.Update();
                htmlFile.Publish("Publish JS File Generation");
            }
        }
    }
}
