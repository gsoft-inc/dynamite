namespace GSoft.Dynamite.Branding
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.SharePoint;

    /// <summary>
    /// Utility to help manage master pages
    /// </summary>
    public interface IMasterPageHelper
    {
        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Web relative Url to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Web relative Url to the custom master page</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Overload with Uri exists. FxCop can't see it.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Overload with Uri exists. FxCop can't see it.")]
        void ApplyMasterPage(SPWeb currentWeb, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Web relative Url to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Web relative Url to the custom master page</param>
        void ApplyMasterPage(SPWeb currentWeb, Uri systemMasterPageWebRelativeUrl, Uri publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Path to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Path to the custom master page</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "Overload with Uri exists. FxCop can't see it.")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Overload with Uri exists. FxCop can't see it.")]
        void ApplyMasterPage(SPSite site, string systemMasterPageWebRelativeUrl, string publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageWebRelativeUrl">Path to the default master page</param>
        /// <param name="publishingMasterPageWebRelativeUrl">Path to the custom master page</param>
        void ApplyMasterPage(SPSite site, Uri systemMasterPageWebRelativeUrl, Uri publishingMasterPageWebRelativeUrl);

        /// <summary>
        /// Applies the master page url on all the web of a site.
        /// </summary>
        /// <param name="site">The web to update.</param>
        /// <param name="systemMasterPageFileName">System MasterPage filename</param>
        /// <param name="publishingMasterPageFileName">Publishing MasterPage filename</param>
        void ApplyRootMasterPage(SPSite site, string systemMasterPageFileName, string publishingMasterPageFileName);

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
        /// <param name="htmlFileName">The filename of the HTML file. This file is supposed to be on the MasterPage gallery root.</param>
        void GenerateMasterPage(SPSite site, string htmlFileName);
    }
}