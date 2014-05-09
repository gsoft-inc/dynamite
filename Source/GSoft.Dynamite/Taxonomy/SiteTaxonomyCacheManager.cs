using Microsoft.SharePoint.Taxonomy;
using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Taxonomy
{
    public class SiteTaxonomyCacheManager : ISiteTaxonomyCacheManager
    {
        private Dictionary<Guid, SiteTaxonomyCache> taxonomyCaches = new Dictionary<Guid, SiteTaxonomyCache>();
        private static readonly NamedReaderWriterLocker<Guid> NamedLocker = new NamedReaderWriterLocker<Guid>();

        public SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName)
        {
            return NamedLocker.RunWithUpgradeableReadLock(site.ID, () =>
            {
                // Create the Site Taxonomy Cache because it does not yet exist.
                if (!this.taxonomyCaches.ContainsKey(site.ID))
                {
                    return NamedLocker.RunWithWriteLock<SiteTaxonomyCache>(site.ID, () =>
                    {
                        // Double check for thread concurency
                        if (!this.taxonomyCaches.ContainsKey(site.ID))
                        {
                            var newTaxCache = new SiteTaxonomyCache(site, termStoreName);
                            this.taxonomyCaches.Add(site.ID, newTaxCache);

                            return newTaxCache;
                        }
                        else
                        {
                            return this.taxonomyCaches[site.ID];
                        }
                    });
                }

                // Return the existing Session
                return this.taxonomyCaches[site.ID];
            });
        }

        //public TaxonomySession RefreshTaxonomySessionCache(SPSite site)
        //{
        //    // Create a new Taxonomy Session with the cache cleared
        //    var newSession = NamedLocker.RunWithWriteLock<TaxonomySession>(site.ID, () =>
        //    {
        //        // This Session will be created and assigned for each thread that passes here.
        //        // A todo would be to check the creation time of the last Taxonomy session was updated in the collection. (a better way must exist...)
        //        var taxonomySession = new TaxonomySession(site, true);

        //        if (this.taxonomyCaches.ContainsKey(site.ID))
        //        {
        //            this.taxonomyCaches[site.ID] = taxonomySession;
        //        }
        //        else
        //        {
        //            this.taxonomyCaches.Add(site.ID, taxonomySession);
        //        }

        //        return taxonomySession;
        //    });

        //    return newSession;
        //}
    }
}
