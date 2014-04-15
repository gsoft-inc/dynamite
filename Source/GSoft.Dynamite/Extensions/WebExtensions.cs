using System.Linq;
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

        /// <summary>
        /// Gets the custom list template with the specified name.
        /// </summary>
        /// <param name="web">The SharePoint web.</param>
        /// <param name="name">The list template name.</param>
        /// <returns>An SPListTemplate or null if nothing is found.</returns>
        public static SPListTemplate GetCustomListTemplate(this SPWeb web, string name)
        {
            var listTemplates = web.Site.GetCustomListTemplates(web);
            var listTemplate = (from SPListTemplate template in listTemplates where template.Name == name select template).FirstOrDefault();
            return listTemplate;
        }
    }
}
