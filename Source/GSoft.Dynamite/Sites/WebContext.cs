using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Minimal information about the current web
    /// </summary>
    public class WebContext : IWebContext
    {
        /// <summary>
        /// Creates a new site collection context from a SharePoint site
        /// collection object
        /// </summary>
        /// <param name="web">The current site</param>
        public WebContext(SPWeb web)
        {
            this.WebId = web.ID;
            this.WebAbsoluteUrl = new Uri(web.Url);
        }

        /// <summary>
        /// Unique ID of the site
        /// </summary>
        public Guid WebId { get; private set; }

        /// <summary>
        /// Absolute URL of the site
        /// </summary>
        public Uri WebAbsoluteUrl { get; private set; }
    }
}
