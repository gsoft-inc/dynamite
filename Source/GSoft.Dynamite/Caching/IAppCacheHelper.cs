using System;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Caching
{
    /// <summary>
    /// General-purpose cache that applies to visitors only
    /// </summary>
    public interface IAppCacheHelper
    {
        /// <summary>
        /// Generic method to place values into cache.
        /// Cache used in both languages and only for visitors.
        /// Current request's LCID used to distinguish cache between both languages.
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <returns>Return value of the function</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> func, ICacheKey key, int expirationInSeconds) where T : class;

        /// <summary>
        /// Generic method to place values into cache
        /// Cache used in both languages and only for visitors
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="func">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <param name="currentUserLcid">Language code for the current request</param>
        /// <returns>Return value of the function</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> func, ICacheKey key, int expirationInSeconds, int currentUserLcid) where T : class;

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        void ClearCache(string keyPrefix);

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="conditionFunc">The conditional function to clear the cache.</param>
        void ClearCache(string keyPrefix, Func<bool> conditionFunc);
    }
}
