namespace GSoft.Dynamite.MasterPages
{
    using System;

    using Microsoft.SharePoint;

    public interface IMasterPageHelper
    {
        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Web relative Url to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Web relative Url to the custom master page</param>
        void ApplyMasterPage(SPWeb currentWeb, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Path to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Path to the custom master page</param>
        void ApplyMasterPage(SPSite site, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on all the web of a site.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageFilename">System MasterPage filename</param>
        /// <param name="publishingMasterPageFilename">Publishing MasterPage filename</param>
        void ApplyRootMasterPage(SPSite site, string systemMasterPageFilename, string publishingMasterPageFilename);

        /// <summary>
        /// Reverts the master page url of a web to its default value.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="originalMasterPath">Path to the original master page to revert to</param>
        [Obsolete]
        void RevertToDefaultMasterPage(SPWeb currentWeb, string originalMasterPath);

        /// <summary>
        /// Reverts the master page url of a web to Seattle.
        /// </summary>
        /// <param name="web">The web to update.</param>
        void RevertToSeattle(SPWeb web);

        /// <summary>
        /// Reverts the master page url of all the web in a site to Seattle.
        /// </summary>
        /// <param name="site">The site containing all the web to update.</param>
        void RevertToSeattle(SPSite site);

        /// <summary>
        /// Generates the master page file corresponding to the HTML file.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <param name="htmlFilename">The filename of the HTML file. This file is supposed to be on the MasterPage gallery root.</param>
        void GenerateMasterPage(SPSite site, string htmlFilename);
    }
}