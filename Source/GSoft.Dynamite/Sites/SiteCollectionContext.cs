using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Sites
{
    /// <summary>
    /// Minimal information about the current site collection
    /// </summary>
    public class SiteCollectionContext : ISiteCollectionContext
    {
        /// <summary>
        /// Creates a new site collection context from a SharePoint site
        /// collection object
        /// </summary>
        /// <param name="site">The current site</param>
        public SiteCollectionContext(SPSite site)
        {
            this.SiteId = site.ID;
            this.SiteAbsoluteUrl = new Uri(site.Url);
        }

        /// <summary>
        /// Unique ID of the site collecton
        /// </summary>
        public Guid SiteId { get; private set; }

        /// <summary>
        /// Absolute URL of the site collection
        /// </summary>
        public Uri SiteAbsoluteUrl { get; private set; }
    }
}
