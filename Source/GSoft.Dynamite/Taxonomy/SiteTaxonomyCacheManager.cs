using System;
using System.Collections.Generic;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// The site taxonomy cache manager.
    /// </summary>
    [Obsolete("Can cause TermStore editing conflicts and stale Term problems. Prefer usage of PerRequestSiteTaxonomyCacheManager.")]
    public class SiteTaxonomyCacheManager : ISiteTaxonomyCacheManager
    {
        private static readonly NamedReaderWriterLocker<Guid> NamedLocker = new NamedReaderWriterLocker<Guid>();

        private readonly Dictionary<Guid, SiteTaxonomyCache> taxonomyCaches = new Dictionary<Guid, SiteTaxonomyCache>();
        private readonly ILogger logger;

        /// <summary>
        /// Constructor with dependencies injection
        /// </summary>
        /// <param name="logger">The logger</param>
        public SiteTaxonomyCacheManager(ILogger logger)
        {
            this.logger = logger;

            this.logger.Info("Creating new SiteTaxonomyCacheManager");
        }

        /// <summary>
        /// The get site taxonomy cache.
        /// </summary>
        /// <param name="site">
        /// The site.
        /// </param>
        /// <param name="termStoreName">
        /// The term store name.
        /// </param>
        /// <returns>
        /// The <see cref="SiteTaxonomyCache"/>.
        /// </returns>
        public SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName)
        {
            return NamedLocker.RunWithUpgradeableReadLock(
                site.ID,
                () =>
                {
                    // Create the Site Taxonomy Cache because it does not yet exist.
                    if (!this.taxonomyCaches.ContainsKey(site.ID))
                    {
                        return NamedLocker.RunWithWriteLock(
                            site.ID,
                            () =>
                            {
                                // Double check for thread concurency
                                if (!this.taxonomyCaches.ContainsKey(site.ID))
                                {
                                    var newTaxCache = new SiteTaxonomyCache(site, termStoreName);

                                    this.logger.Info("SiteTaxonomyCacheManager: Adding site taxonomy cache for site collection " + site.Url);
                                    this.taxonomyCaches.Add(site.ID, newTaxCache);

                                    return newTaxCache;
                                }

                                return this.taxonomyCaches[site.ID];
                            });
                    }

                    // Return the existing Session
                    return this.taxonomyCaches[site.ID];
                });
        }
    }
}
