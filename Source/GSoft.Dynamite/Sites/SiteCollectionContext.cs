using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

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

            string termStoreName = string.Empty;

            if (site.RootWeb.AllProperties["TermStoreName"] != null)
            {
                termStoreName = site.RootWeb.AllProperties["TermStoreName"].ToString();
            }

            var session = new TaxonomySession(site);
            TermStore store = null;

            if (string.IsNullOrWhiteSpace(termStoreName))
            {
                store = session.DefaultSiteCollectionTermStore;
            }
            else
            {
                store = session.TermStores[termStoreName];
            }

            if (store != null)
            {
                this.ContextTermStore = new TermStoreInfo(store);
            }
            else
            {
                new TraceLogger("GSoft.Dynamite", "GSoft.Dynamite", false)
                    .Error("SiteCollectionContext.ctor: Failed to resolve current term store. " +
                    "Please register the name of your managed metadata service on your root web property bag under the key 'TermStoreName'. " +
                    "Alternatively, under Manage Service Applications, configure your managed metadata service connection's properties to " +
                    "make it the 'Default storage location for column specific term sets.'");
            }
        }

        /// <summary>
        /// Unique ID of the site collecton
        /// </summary>
        public Guid SiteId { get; private set; }

        /// <summary>
        /// Absolute URL of the site collection
        /// </summary>
        public Uri SiteAbsoluteUrl { get; private set; }

        /// <summary>
        /// The metadata of the default term store connected to the
        /// site collection.
        /// </summary>
        public TermStoreInfo ContextTermStore { get; private set; }
    }
}
