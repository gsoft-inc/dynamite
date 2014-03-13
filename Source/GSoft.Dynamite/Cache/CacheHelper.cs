using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using GSoft.Dynamite;
using GSoft.Dynamite.Logging;

using Microsoft.SharePoint;

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// General-purpose cache that applies to visitors only
    /// </summary>
    public class CacheHelper : ICacheHelper
    {
        private ILogger log;

        /// <summary>
        /// Creates a cache helper
        /// </summary>
        /// <param name="log">The logger</param>
        public CacheHelper(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// The cache key should discriminate between language and encode the user's
        /// security groups (so that members of the same group(s) feed off a common cache).
        /// Current request's LCID used to distinguish cache between both languages.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="repoCamlQuery">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> repoCamlQuery, ICacheKey key, int expirationInSeconds) where T : class
        {
            return this.Get<T>(repoCamlQuery, key, expirationInSeconds, CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// The cache key should discriminate between language and encode the user's
        /// security groups (so that members of the same group(s) feed off a common cache).
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="repoCamlQuery">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <param name="currentUserLCID">Language code for the current request</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> repoCamlQuery, ICacheKey key, int expirationInSeconds, int currentUserLCID) where T : class
        {
            // Define the cache key based on the user's current language
            string cacheKey = string.Empty;
            DateTime expiration = DateTime.Now.AddSeconds(expirationInSeconds);

            if (currentUserLCID == Language.French.Culture.LCID)
            {
                cacheKey = key.InFrench;
            }
            else
            {
                cacheKey = key.InEnglish;
            }

            // Note that caching is only possible if we currently have a valid HttpContext.
            if (HttpContext.Current != null)
            {
                // Note that the cache key should take care of discriminating between different security groups,
                // such that users that belong to different groups feed of different caches.
                // The big assumption here is that everyone belongs to a group (or is anonymous) and isn't given 
                // personal permissions on various securables.
                return this.GetFromCache<T>(repoCamlQuery, cacheKey, expiration);
            }
            else
            {
                // Skip the cache since there is no httpcontext
                return repoCamlQuery.Invoke();
            }
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <returns>The number of keys cleared from cache</returns>
        public int ClearCache()
        {
            this.log.Info("Clearing Dynamite CacheHelper cached items.");

            int clearCount = 0;

            if (SPContext.Current.Web.CurrentUser != null
                && SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.FullMask))
            {
                HttpRuntime.Cache.Cast<DictionaryEntry>()
                    .Where(entry => entry.Key.ToString().StartsWith(SimpleCacheKey.Prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(entry => entry.Key.ToString()).ToList()
                    .ForEach(key => 
                        {
                            clearCount++;
                            HttpRuntime.Cache.Remove(key);
                        });

                this.log.Info("Cleared {0} keys form HttpCache.", clearCount);
            }
            else
            {
                throw new InvalidOperationException("Can't clear cache if you don't have FullControl permission.");
            }

            return clearCount;
        }

        private T GetFromCache<T>(Func<T> repoCamlQuery, string cacheKey, DateTime expiration) where T : class
        {
            T cachedValue = HttpRuntime.Cache.Get(cacheKey) as T;
            if (cachedValue == null)
            {
                this.log.Info("Caching value(s) for key = " + cacheKey);
                cachedValue = repoCamlQuery.Invoke();

                if (cachedValue != null)
                {
                    // Add item to cache
                    HttpRuntime.Cache.Add(
                        cacheKey,
                        cachedValue,
                        null,
                        expiration,
                        System.Web.Caching.Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null);
                }
            }

            return cachedValue;
        }
    }
}
