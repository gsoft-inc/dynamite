using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Caching;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Caching
{
    /// <summary>
    /// General-purpose application cache
    /// </summary>
    public class AppCacheHelper : IAppCacheHelper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a cache helper
        /// </summary>
        /// <param name="logger">The logger</param>
        public AppCacheHelper(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// Current request's LCID used to distinguish cache between both languages.
        /// Note: Cached objects must be serializable.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> func, ICacheKey key, int expirationInSeconds) where T : class
        {
            return this.Get<T>(func, key, expirationInSeconds, CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// Note: Cached objects must be serializable.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <param name="currentUserLcid">Language code for the current request</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> func, ICacheKey key, int expirationInSeconds, int currentUserLcid) where T : class
        {
            // Define the cache key based on the user's current language
            var expiration = DateTime.Now.AddSeconds(expirationInSeconds);
            var cacheKey = currentUserLcid == Language.French.Culture.LCID ? key.InFrench : key.InEnglish;

            // Note that caching is only possible if we currently have a valid HttpContext.
            this._logger.Info("Getting app cache value(s) for key '{0}'.", cacheKey);
            return HttpContext.Current != null ? this.GetFromCache<T>(func, cacheKey, expiration) : func.Invoke();
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        public void ClearCache(string keyPrefix)
        {
            this.ClearCache(keyPrefix, () => true);
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="conditionFunc">The conditional function to clear the cache.</param>
        /// <exception cref="System.InvalidOperationException">Can't clear cache if you don't have ApproveItems permission.</exception>
        public void ClearCache(string keyPrefix, Func<bool> conditionFunc)
        {
            this._logger.Info("Clearing app cache.");

            if (conditionFunc.Invoke())
            {
                HttpRuntime.Cache.Cast<DictionaryEntry>()
                    .Where(entry => entry.Key.ToString().StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                    .Select(entry => entry.Key.ToString()).ToList()
                    .ForEach(key => HttpRuntime.Cache.Remove(key));
            }
            else
            {
                throw new InvalidOperationException("Can't clear app cache because of condition.");
            }
        }

        private T GetFromCache<T>(Func<T> func, string cacheKey, DateTime expiration) where T : class
        {
            var cachedValue = HttpRuntime.Cache.Get(cacheKey) as T;
            if (cachedValue == null)
            {
                this._logger.Info("Not found in app cache. Caching value(s) for key '{0}'", cacheKey);
                cachedValue = func.Invoke();

                if (cachedValue != null)
                {
                    // Add item to cache
                    HttpRuntime.Cache.Add(
                        cacheKey,
                        cachedValue,
                        null,
                        expiration,
                        Cache.NoSlidingExpiration,
                        CacheItemPriority.Normal,
                        null);
                }
            }

            return Cloner.BinaryClone(cachedValue);
        }
    }
}
