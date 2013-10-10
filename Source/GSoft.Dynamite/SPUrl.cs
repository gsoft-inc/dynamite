using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Sharepoint2013
{
    /// <summary>
    /// A class for encapsulating a SharePoint URL.
    /// </summary>
    public class SPUrl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SPUrl"/> class.
        /// </summary>
        /// <param name="web">The web the URL targets.</param>
        /// <param name="webRelativeUrl">The web relative URL.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This constructor's responsibility is to parse the string into proper Uris")]
        public SPUrl(SPWeb web, string webRelativeUrl)
        {
            var webRelative = new Uri(webRelativeUrl, UriKind.Relative);
            this.AbsoluteUrl = new Uri(SPUtility.ConcatUrls(web.Url, webRelative.ToString()), UriKind.Absolute);
            this.ServerRelativeUrl = new Uri(this.AbsoluteUrl.LocalPath, UriKind.Relative);
            this.WebRelativeUrl = webRelative;
        }

        /// <summary>
        /// Gets the absolute URL.
        /// </summary>
        public Uri AbsoluteUrl { get; private set; }

        /// <summary>
        /// Gets the server relative URL.
        /// </summary>
        public Uri ServerRelativeUrl { get; private set; }

        /// <summary>
        /// Gets the web relative URL.
        /// </summary>
        public Uri WebRelativeUrl { get; private set; }
    }
}
