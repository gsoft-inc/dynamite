using System;
using System.Collections.Generic;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Helpers
{
    /// <summary>
    /// Helper class for display template work
    /// </summary>
    public class DisplayTemplateHelper
    {
        /// <summary>
        /// Folder name for Display Templates
        /// </summary>
        public readonly string DisplayTemplatesFolder = "Display Templates";

        /// <summary>
        /// Folder name for Content WebPart Folder
        /// </summary>
        public readonly string ContentWebPartFolder = "Content Web Parts";

        /// <summary>
        /// Folder name for Search 
        /// </summary>
        public readonly string SearchFolder = "Search";

        /// <summary>
        /// Folder name for Filter
        /// </summary>
        public readonly string FilterFolder = "Filters";

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
