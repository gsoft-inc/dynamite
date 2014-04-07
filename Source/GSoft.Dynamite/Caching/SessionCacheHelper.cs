using System;
using System.Globalization;
using System.Web;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.Utils;

namespace GSoft.Dynamite.Caching
{
    /// <summary>
    /// General-purpose application cache
    /// </summary>
    public class SessionCacheHelper : ISessionCacheHelper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Creates a cache helper
        /// </summary>
        /// <param name="logger">The logger</param>
        public SessionCacheHelper(ILogger logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// Cache used in both languages.
        /// Note: Cached objects must be serializable.
        /// Current request's LCID used to distinguish cache between both languages.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> func, ICacheKey key) where T : class
        {
            return this.Get(func, key, CultureInfo.CurrentUICulture.LCID);
        }

        /// <summary>
        /// Generic method to place values into cache.
        /// Cache used in both languages.
        /// Note: Cached objects must be serializable.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="currentUserLcid">Language code for the current request</param>
        /// <returns>Return value of the function</returns>
        public T Get<T>(Func<T> func, ICacheKey key, int currentUserLcid) where T : class
        {
            // Define the cache key based on the user's current language
            var cacheKey = currentUserLcid == Language.French.Culture.LCID ? key.InFrench : key.InEnglish;

            // Note that caching is only possible if we currently have a valid HttpContext.
            this._logger.Info("Get: Getting session cache value(s) for key '{0}'.", cacheKey);
            return HttpContext.Current != null ? this.GetFromCache<T>(func, cacheKey) : func.Invoke();
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        public void ClearCache()
        {
            this.ClearCache(() => true);
        }

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <param name="conditionFunc">The conditional function to clear the cache.</param>
        /// <exception cref="System.InvalidOperationException">Can't clear session cache because condition is not met.</exception>
        public void ClearCache(Func<bool> conditionFunc)
        {
            this._logger.Info("ClearCache: Clearing session cache.");

            if (conditionFunc.Invoke())
            {
                if (HttpContext.Current.Session != null)
                {
                    HttpContext.Current.Session.Clear();
                }
                else
                {
                    this._logger.Error("ClearCache: Cache session is null.  Please enable session state service using the 'Enable-SPSessionStateService' cmdlet.");
                }
            }
            else
            {
                throw new InvalidOperationException("Can't clear session cache because condition is not met.");
            }
        }

        private T GetFromCache<T>(Func<T> func, string cacheKey) where T : class
        {
            if (HttpContext.Current.Session != null)
            {
                var cachedValue = HttpContext.Current.Session[cacheKey] as T;
                if (cachedValue == null)
                {
                    this._logger.Info("GetFromCache: Not found in session cache. Caching value(s) for key '{0}'", cacheKey);
                    cachedValue = func.Invoke();

                    if (cachedValue != null)
                    {
                        HttpContext.Current.Session.Add(cacheKey, cachedValue);
                    }
                }

                // Must return cloned object or else reference points to cached object
                return Cloner.BinaryClone(cachedValue);
            }
            else
            {
                this._logger.Error("GetFromCache: Cache session is null.  Please enable session state service using the 'Enable-SPSessionStateService' cmdlet.");
            }

            return null;
        }
    }
}
