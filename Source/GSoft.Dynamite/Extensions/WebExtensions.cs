using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Extensions
{
    /// <summary>
    /// Extensions for the SPWeb type.
    /// </summary>
    public static class WebExtensions
    {
        /// <summary>
        /// Gets the pages library.
        /// </summary>
        /// <param name="web">The web to get the Pages library from.</param>
        /// <exception cref="System.ArgumentException">No Pages library was found for this web.</exception>
        /// <returns>The Pages library.</returns>
        public static SPList GetPagesLibrary(this SPWeb web)
        {
            return web.GetList(SPUtility.ConcatUrls(web.ServerRelativeUrl, SPUtility.GetLocalizedString("$Resources:List_Pages_UrlName", "osrvcore", web.Language)));
        }
    }
}
