using System;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Branding
{
    /// <summary>
    /// Provides utility methods for creating a setup for a custom master page within SharePoint.
    /// </summary>
    public class MasterPageHelper : IMasterPageHelper
    {
        private readonly ILogger logger;
        private readonly string seattleMasterPageFileName = "seattle.master";

        /// <summary>
        /// Constructor for dependencies injection
        /// </summary>
        /// <param name="logger">The Logger</param>
        public MasterPageHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Web relative Url to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Web relative Url to the custom master page</param>
        public void ApplyMasterPage(SPWeb currentWeb, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl)
        {
            this.ApplyMasterPage(currentWeb, new Uri(systemMasterPageWebRelativeUrl, UriKind.Relative), new Uri(publishingMasterPageWebRelativeUrl, UriKind.Relative));
        }

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Web relative Url to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Web relative Url to the custom master page</param>
        public void ApplyMasterPage(SPWeb currentWeb, Uri systemMasterPageWebRelativeUrl, Uri publishingMasterPageWebRelativeUrl)
        {
            // Be sure to use the root web to forge the url
            var rootWeb = currentWeb.Site.RootWeb;

            var systemMasterPageUri = !systemMasterPageWebRelativeUrl.IsAbsoluteUri ? new SPUrl(rootWeb, systemMasterPageWebRelativeUrl.ToString()).AbsoluteUrl : null;
            var publishingMasterPageUri = !publishingMasterPageWebRelativeUrl.IsAbsoluteUri ? new SPUrl(rootWeb, publishingMasterPageWebRelativeUrl.ToString()).AbsoluteUrl : null;
            UpdateMasterPages(currentWeb, systemMasterPageUri, publishingMasterPageUri);
        }

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Path to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Path to the custom master page</param>
        public void ApplyMasterPage(SPSite site, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl)
        {
            this.ApplyMasterPage(site, new Uri(systemMasterPageWebRelativeUrl, UriKind.Relative), new Uri(publishingMasterPageWebRelativeUrl, UriKind.Relative));
        }

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Path to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Path to the custom master page</param>
        public void ApplyMasterPage(SPSite site, Uri systemMasterPageWebRelativeUrl, Uri publishingMasterPageWebRelativeUrl)
        {
            var systemMasterPageUri = !systemMasterPageWebRelativeUrl.IsAbsoluteUri ? new SPUrl(site.RootWeb, systemMasterPageWebRelativeUrl.ToString()).AbsoluteUrl : null;
            var publishingMasterPageUri = !publishingMasterPageWebRelativeUrl.IsAbsoluteUri ? new SPUrl(site.RootWeb, publishingMasterPageWebRelativeUrl.ToString()).AbsoluteUrl : null;
            UpdateMasterPages(site, systemMasterPageUri, publishingMasterPageUri);
        }

        /// <summary>
        /// Applies the master page url on all the web of a site.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageFileName">System MasterPage filename</param>
        /// <param name="publishingMasterPageFileName">Publishing MasterPage filename</param>
        public void ApplyRootMasterPage(SPSite site, string systemMasterPageFileName, string publishingMasterPageFileName)
        {
            Uri systemMasterPageUri = null;
            Uri publishingMasterPageUri = null;

            if (!string.IsNullOrEmpty(systemMasterPageFileName))
            {
                systemMasterPageUri = new SPUrl(site.RootWeb, this.GetSiteRelativeMasterPageUrl(site, systemMasterPageFileName).Url).AbsoluteUrl;
            }

            if (!string.IsNullOrEmpty(publishingMasterPageFileName))
            {
                publishingMasterPageUri = new SPUrl(site.RootWeb, this.GetSiteRelativeMasterPageUrl(site, publishingMasterPageFileName).Url).AbsoluteUrl;
            }

            UpdateMasterPages(site, systemMasterPageUri, publishingMasterPageUri);
        }

        /// <summary>
        /// Reverts the master page url of a web to Seattle.
        /// </summary>
        /// <param name="web">The web to update.</param>
        public void RevertToSeattle(SPWeb web)
        {
            var masterPageFile = this.GetSiteRelativeMasterPageUrl(web.Site, this.seattleMasterPageFileName);
            var seattleMasterPageUri = new SPUrl(web, masterPageFile.Url).AbsoluteUrl;
            UpdateMasterPages(web, seattleMasterPageUri, seattleMasterPageUri);
        }

        /// <summary>
        /// Reverts the master page url of all the web in a site to Seattle.
        /// </summary>
        /// <param name="site">The site containing all the web to update.</param>
        public void RevertToSeattle(SPSite site)
        {
            var masterPageFile = this.GetSiteRelativeMasterPageUrl(site, this.seattleMasterPageFileName);
            var seattleMasterPageUri = new SPUrl(site.RootWeb, masterPageFile.Url).AbsoluteUrl;
            UpdateMasterPages(site, seattleMasterPageUri, seattleMasterPageUri);
        }

        /// <summary>
        /// Generates the master page file corresponding to the HTML file.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="htmlFileName">The filename of the HTML file. This file is supposed to be on the MasterPage gallery root.</param>
        public void GenerateMasterPage(SPSite site, string htmlFileName)
        {
            if (string.IsNullOrEmpty(htmlFileName) || !(htmlFileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || htmlFileName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("The htmlFilename argument is null of empty and should finish by '.html' or '.htm' .");
            }

            SPFile htmlFile = null;

            try
            {
                htmlFile = this.GetSiteRelativeMasterPageUrl(site, htmlFileName);

                // undo the customization, necessary only upon successive feature re-activations 
                // (because the Checkout and edits below cause the unghosting/customization of the file)
                htmlFile.RevertContentStream();
            }
            catch (SPException e)
            {
                this.logger.Warn("Failed to undo customization while re-provisioning HTML design file. Exception: {0} StackTrace: {1}", e.Message, e.StackTrace);
            }

            if (htmlFile != null)
            {
                htmlFile.CheckOut();
                htmlFile.Update();

                htmlFile.CheckIn("Generate masterpage File");
                htmlFile.Update();

                htmlFile.Publish("Publish masterpage file generation");

                this.logger.Info("Master Page with Url: '{0}' was successfully generated.", htmlFileName);
            }
        }

        /// <summary>
        /// Method to get the MasterPage SPFile from a fileName and the SPSite
        /// RootFolder.Files is a SPFileCollection and the indexer [] throws an ArgumentException if the fileName is not found.
        /// We can't use web.GetFile() for a catalog file.
        /// Log the error and return null.
        /// </summary>
        /// <param name="site">The current Site Collection where the MasterPage Gallery lives</param>
        /// <param name="fileName">The filename of the MasterPage we are looking for. With the extension. Ex: seattle.master</param>
        /// <returns>A SPFile if found, null if not found.</returns>
        private SPFile GetSiteRelativeMasterPageUrl(SPSite site, string fileName)
        {
            SPFile masterPageFile = null;
            var masterPageCatalog = site.GetCatalog(SPListTemplateType.MasterPageCatalog);

            try
            {
                masterPageFile = masterPageCatalog.RootFolder.Files[fileName];
            }
            catch (ArgumentException e)
            {
                this.logger.Warn("The file with filename '{0}' was not found in the master page gallery. StackTrace: {1}", fileName, e.StackTrace);
            }

            return masterPageFile;
        }

        private static void UpdateMasterPages(SPWeb web, Uri systemMasterPageUri, Uri publishingMasterPageUri)
        {
            if (systemMasterPageUri != null)
            {
                web.MasterUrl = systemMasterPageUri.AbsolutePath;
            }

            if (publishingMasterPageUri != null)
            {
                web.CustomMasterUrl = publishingMasterPageUri.AbsolutePath;
            }

            web.Update();
        }

        private static void UpdateMasterPages(SPSite site, Uri systemMasterPageUri, Uri publishingMasterPageUri)
        {
            foreach (SPWeb web in site.AllWebs)
            {
                UpdateMasterPages(web, systemMasterPageUri, publishingMasterPageUri);
            }
        }
    }
}
