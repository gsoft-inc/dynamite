using Microsoft.SharePoint.Taxonomy;
using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Taxonomy
{
    public class TaxonomySessionManager : ITaxonomySessionManager
    {
        private Dictionary<Guid, TaxonomySession> taxonomySessions = new Dictionary<Guid,TaxonomySession>();
        private static readonly NamedReaderWriterLocker<Guid> NamedLocker = new NamedReaderWriterLocker<Guid>();

        public TaxonomySession GetSession(SPSite site)
        {
            var session = NamedLocker.RunWithUpgradeableReadLock(site.ID, () =>
            {
                if (!this.taxonomySessions.ContainsKey(site.ID))
                {
                    // Create the Session because it does not yet exist.
                    var newSession = NamedLocker.RunWithWriteLock<TaxonomySession>(site.ID, () =>
                    {
                        // Double check for thread concurency
                        if (!this.taxonomySessions.ContainsKey(site.ID))
                        {
                            var taxonomySession = new TaxonomySession(site, true);
                            this.taxonomySessions.Add(site.ID, taxonomySession);

                            return taxonomySession;
                        }
                        else
                        {
                            return this.taxonomySessions[site.ID];
                        }
                    });

                    return newSession;
                }

                // Return the existing Session
                return this.taxonomySessions[site.ID];
            });

            return session;
        }

        public TaxonomySession RefreshTaxonomySessionCache(SPSite site)
        {
            // Create a new Taxonomy Session with the cache cleared
            var newSession = NamedLocker.RunWithWriteLock<TaxonomySession>(site.ID, () =>
            {
                // This Session will be created and assigned for each thread that passes here.
                // A todo would be to check the creation time of the last Taxonomy session was updated in the collection. (a better way must exist...)
                var taxonomySession = new TaxonomySession(site, true);

                if (this.taxonomySessions.ContainsKey(site.ID))
                {
                    this.taxonomySessions[site.ID] = taxonomySession;
                }
                else
                {
                    this.taxonomySessions.Add(site.ID, taxonomySession);
                }

                return taxonomySession;
            });

            return newSession;
        }
    }
}
