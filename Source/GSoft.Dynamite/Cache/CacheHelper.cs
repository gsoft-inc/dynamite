using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;

using GSoft.Dynamite.Logging;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// General-purpose cache that applies to visitors only
    /// </summary>
    public class CacheHelper : ICacheHelper
    {
        private ILogger logger;

        /// <summary>
        /// Creates a cache helper
        /// </summary>
        /// <param name="logger">The logger</param>
        public CacheHelper(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// The cache key should discriminate between language and encode the user's
        /// security groups (so that members of the same group(s) feed off a common cache).
        /// Current request's LCID used to distinguish cache between both languages.
        /// If the method invocation returns NULL, then a NULL result will be cached
        /// and returned until cache expiration.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="methodToInvoke">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds) where T : class
        {
            return this.Get<T>(methodToInvoke, key, expirationInSeconds, CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// The cache key should discriminate between language and encode the user's
        /// security groups (so that members of the same group(s) feed off a common cache).
        /// If the method invocation returns NULL, then a NULL result will be cached
        /// and returned until cache expiration.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="methodToInvoke">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <param name="currentUserLCID">Language code for the current request</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds, int currentUserLCID) where T : class
        {
            // By default, NULL values are allowed
            return this.Get<T>(methodToInvoke, key, expirationInSeconds, currentUserLCID, true);
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// The cache key should discriminate between language and encode the user's
        /// security groups (so that members of the same group(s) feed off a common cache).
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="methodToInvoke">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <param name="currentUserLCID">Language code for the current request</param>
        /// <param name="isNullValueAllowedInCache">Whether null values should be cached or not</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds, int currentUserLCID, bool isNullValueAllowedInCache) where T : class
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
                return this.GetFromCache<T>(methodToInvoke, cacheKey, expiration, isNullValueAllowedInCache);
            }
            else
            {
                // Skip the cache since there is no httpcontext
                return methodToInvoke.Invoke();
            }
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <returns>The number of keys cleared from cache</returns>
        public int ClearCache()
        {
            this.logger.Info("Clearing Dynamite CacheHelper cached items.");

            int clearCount = 0;

            if (SPContext.Current.Web.CurrentUser != null && SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.FullMask))
            {
                HttpRuntime.Cache.Cast<DictionaryEntry>()
                    .Where(entry => entry.Key.ToString().StartsWith(SimpleCacheKey.Prefix, StringComparison.OrdinalIgnoreCase))
                    .Select(entry => entry.Key.ToString()).ToList()
                    .ForEach(key =>
                        {
                            clearCount++;
                            this.ClearCache(key);
                        });

                this.logger.Info("Cleared {0} keys form HttpCache.", clearCount);
            }
            else
            {
                throw new InvalidOperationException("Can't clear cache if you don't have FullControl permission.");
            }

            return clearCount;
        }

        /// <summary>
        /// Clear the cached information for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lcid">The language of the cache. If null (default) is passed the current UI culture is used.</param>
        public void ClearCache(ICacheKey key, int? lcid = null)
        {
            string cacheKey = string.Empty;
            lcid = lcid ?? CultureInfo.CurrentUICulture.LCID;

            if (lcid.Value == Language.French.Culture.LCID)
            {
                cacheKey = key.InFrench;
            }
            else
            {
                cacheKey = key.InEnglish;
            }

            this.ClearCache(cacheKey);
        }

        /// <summary>
        /// Clear the cached information for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void ClearCache(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            HttpRuntime.Cache.Remove(key);
        }

        private T GetFromCache<T>(Func<T> repoCamlQuery, string cacheKey, DateTime expiration, bool isNullValueAllowedInCache) where T : class 
        {
            var cachedValue = HttpRuntime.Cache.Get(cacheKey) as CacheItemWrapper<T>;

            if (cachedValue == null)
            {
                this.logger.Info("Caching value(s) for key = " + cacheKey);

                T result = repoCamlQuery.Invoke();

                if (result == null && !isNullValueAllowedInCache)
                {
                    // NULLs are forbidden in cache, so we should skip this result.
                    return null;
                }

                // Wrap the result to support caching of NULL results
                cachedValue = new CacheItemWrapper<T>(result);
                
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
            
            return cachedValue.Item;
        }
    }
}
