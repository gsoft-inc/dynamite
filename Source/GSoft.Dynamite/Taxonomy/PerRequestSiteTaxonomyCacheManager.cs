using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// The site taxonomy cache manager.
    /// </summary>
    public class PerRequestSiteTaxonomyCacheManager : ISiteTaxonomyCacheManager
    {
        private const string KeyPrefix = "PerRequestSiteTaxonomyCacheManager_";
        private ITaxonomyHelper taxonomyHelper;

        /// <summary>
        /// Per-request taxonomy cache manager (using HttpContext.Items)
        /// </summary>
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        public PerRequestSiteTaxonomyCacheManager(ITaxonomyHelper taxonomyHelper)
        {
            this.taxonomyHelper = taxonomyHelper;
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
        /// <param name="taxonomyHelper">The taxonomy helper.</param>
        /// <returns>
        /// The <see cref="SiteTaxonomyCache"/>.
        /// </returns>
        public SiteTaxonomyCache GetSiteTaxonomyCache(SPSite site, string termStoreName, ITaxonomyHelper taxonomyHelper)
        {
            // No caching if outside HttpContext
            if (HttpContext.Current == null)
            {
                return new SiteTaxonomyCache(site, termStoreName, taxonomyHelper);
            }

            string cacheKey = KeyPrefix + site.ID.ToString();

            // Create the Site Taxonomy Cache because it does not yet exist. No need for locking because
            // we only cache per-request using the HttpContext cache.
            if (HttpContext.Current.Items[cacheKey] == null)
            {
                var newTaxCache = new SiteTaxonomyCache(site, termStoreName, this.taxonomyHelper);
                HttpContext.Current.Items[cacheKey] = newTaxCache;
            }

            // Return the existing Session
            return (SiteTaxonomyCache)HttpContext.Current.Items[cacheKey];
        }
    }
}
