using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.MasterPages
{
    /// <summary>
    /// Small utility to help in linking to minified JavaScript or CSS files
    /// </summary>
    public static class Minification
    {
        /// <summary>
        /// Adds MIN prefix to any CSS or JS extension in Release mode.
        /// </summary>
        /// <param name="pathToMinify">The URL containing a CSS or JS file extension</param>
        /// <returns>The modified URL, if Release mode.</returns>
        public static string MinifyPathIfNotDebug(string pathToMinify)
        {
#if DEBUG
            // To help with debugging, Css or Script files should not be minified.
            return pathToMinify;
#else
            // In Release, minified files should be used
            return pathToMinify.Replace(".css", ".min.css").Replace(".js", ".min.js");
#endif
        }

        /// <summary>
        /// Generates the CSS registration. CSS link elements need to be generated on the fly - rewriting their arguments from code-behind doesn't work.
        /// </summary>
        /// <param name="site">The current Site</param>
        /// <param name="serverRelativeCssUrl">The server relative CSS URL.</param>
        /// <returns>A CSS registration control.</returns>
        public static CssRegistration GenerateCssRegistration(SPSite site, string serverRelativeCssUrl)
        {
            return GenerateCssRegistration(site.ServerRelativeUrl, serverRelativeCssUrl);
        }

        /// <summary>
        /// Generates the CSS registration. CSS link elements need to be generated on the fly - rewriting their arguments from code-behind doesn't work.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative url</param>
        /// <param name="serverRelativeCssUrl">The server relative CSS URL.</param>
        /// <returns>A CSS registration control.</returns>
        public static CssRegistration GenerateCssRegistration(string serverRelativeUrl, string serverRelativeCssUrl)
        {
            return GenerateCssRegistration(serverRelativeUrl, serverRelativeCssUrl, true);
        }

        /// <summary>
        /// Generates the CSS registration. CSS link elements need to be generated on the fly - rewriting their arguments from code-behind doesn't work.
        /// </summary>
        /// <param name="serverRelativeUrl">The server relative url</param>
        /// <param name="serverRelativeCssUrl">The server relative CSS URL.</param>
        /// <param name="useVersionTag">Boolean to override the use of a version tag. It's easier to break into JavaScript if the version doesn't change on every load.</param>
        /// <returns>A CSS registration control.</returns>
        public static CssRegistration GenerateCssRegistration(string serverRelativeUrl, string serverRelativeCssUrl, bool useVersionTag)
        {
            var cssUrl = useVersionTag ? serverRelativeCssUrl + "?v=" + VersionContext.CurrentVersionTag : serverRelativeCssUrl;

            return new CssRegistration()
            {
                Name = MinifyPathIfNotDebug(SPUtility.ConcatUrls(serverRelativeUrl, cssUrl))
            };
        }
    }
}
