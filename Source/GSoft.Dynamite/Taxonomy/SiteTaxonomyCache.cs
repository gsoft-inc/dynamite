using System;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// The site taxonomy cache.
    /// </summary>
    public class SiteTaxonomyCache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteTaxonomyCache"/> class.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        public SiteTaxonomyCache(SPSite site) : this(site, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteTaxonomyCache"/> class.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        public SiteTaxonomyCache(SPSite site, string termStoreName)
        {
            using (new SPMonitoredScope("GSoft.Dynamite - Site taxonomy cache initialization"))
            {
                if (site == null)
                {
                    throw new ArgumentNullException("site", "SPSite is currently null, please pass a valid site as argument.");
                }

                this.SiteId = site.ID;

                // Don't send in the updateCache=true setting - let the SharePoint inner Taxonomy cache refresh itself normally (every 10 seconds or so)
                this.TaxonomySession = new TaxonomySession(site);

                if (!string.IsNullOrEmpty(termStoreName))
                {
                    this.SiteCollectionGroup = this.TaxonomySession.TermStores[termStoreName].GetSiteCollectionGroup(site);
                }
                else
                {
                    // Use default term store
                    this.SiteCollectionGroup = this.TaxonomySession.DefaultSiteCollectionTermStore.GetSiteCollectionGroup(site);
                }
            }
        }

        /// <summary>
        /// Gets or sets the site id.
        /// </summary>
        public Guid SiteId { get; private set; }

        /// <summary>
        /// Gets or sets the taxonomy session.
        /// </summary>
        public TaxonomySession TaxonomySession { get; private set; }

        /// <summary>
        /// Gets or sets the site collection group.
        /// </summary>
        public Group SiteCollectionGroup { get; private set; }
    }
}
