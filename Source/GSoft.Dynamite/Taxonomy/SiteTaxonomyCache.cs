using System;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    public class SiteTaxonomyCache
    {
        public SiteTaxonomyCache(SPSite site) : this(site, null)
        {
        }

        public SiteTaxonomyCache(SPSite site, string termStoreName)
        {
            this.SiteId = site.ID;

            // Does not refresh sharepoint internal cache by default.
            this.TaxonomySession = new TaxonomySession(site, true);

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

        public Guid SiteId { get; set; }

        public TaxonomySession TaxonomySession { get; set; }

        public Group SiteCollectionGroup { get; set; }
    }
}
