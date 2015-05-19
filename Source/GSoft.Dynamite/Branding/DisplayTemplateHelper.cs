using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;

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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "SPFile-generated exceptions are hard to anticipate. General exceptions are properly logged here.")]
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

            if (htmlFiles.Count > 0)
            {
                // Flush the blob cache accross the entire web application (otherwise the old 
                // version of the Display Template will stay stuck in the cache, especially when
                // your Display Templates are associated with Result Types)
                try
                {
                    // WARNING: You need security_admin SQL server role and the db_owner role on the web app's content DB
                    // in order to successfully flush the web app's BLOB cache.
                    PublishingCache.FlushBlobCache(htmlFiles[0].ParentFolder.ParentWeb.Site.WebApplication);
                }
                catch (SPException exception)
                {
                    this.logger.Error("DisplayTemplateHelper.GenerateJavascriptFile: Failed to flush the BLOB cache accross the web app. You need You need security_admin SQL server role and the db_owner role on the web app's content DB. Caught and swallowed exception: {0}", exception);
                }
            }
        }
    }
}
