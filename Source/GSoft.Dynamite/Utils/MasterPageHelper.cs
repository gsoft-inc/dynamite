using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Sharepoint2013.Utils
{
    /// <summary>
    /// Provides utility methods for creating a setup for a custom master page within SharePoint.
    /// </summary>
    [CLSCompliant(false)]
    public class MasterPageHelper
    {
        /// <summary>
        /// Applies the master page url on a web.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="masterPath">Path to the default master page</param>
        /// <param name="customMasterPath">Path to the custom master page</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "SPUrl utility is meant to take care of the string-Uri conversion.")]
        public void ApplyMasterPage(SPWeb currentWeb, string masterPath, string customMasterPath)
        {
            SPWeb rootWeb = currentWeb.Site.RootWeb;
            Uri masterUri = new SPUrl(rootWeb, masterPath).AbsoluteUrl;
            Uri customUri = new SPUrl(rootWeb, customMasterPath).AbsoluteUrl;
            UpdateWebMasterPages(currentWeb, masterUri, customUri);
        }

        /// <summary>
        /// Reverts the master page url of a web to its default value.
        /// </summary>
        /// <param name="currentWeb">The web to update.</param>
        /// <param name="originalMasterPath">Path to the original master page to revert to</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Use of statics is discouraged - this favors more flexibility and consistency with dependency injection.")]
        [SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "SPUrl utility is meant to take care of the string-Uri conversion.")]
        public void RevertToDefaultMasterPage(SPWeb currentWeb, string originalMasterPath)
        {
            SPWeb rootWeb = currentWeb.Site.RootWeb;
            Uri masterUri = new SPUrl(rootWeb, originalMasterPath).AbsoluteUrl;
            UpdateWebMasterPages(currentWeb, masterUri, masterUri);
        }

        private static void UpdateWebMasterPages(SPWeb web, Uri masterUri, Uri customUri)
        {
            web.MasterUrl = masterUri.AbsolutePath;
            web.CustomMasterUrl = customUri.AbsolutePath;
            web.Update();
        }
    }
}
