// -----------------------------------------------------------------------
// <copyright file="ICacheHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Cache
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// General-purpose cache that applies to visitors only
    /// </summary>
    public interface ICacheHelper
    {
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
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds) where T : class;

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
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds, int currentUserLCID) where T : class;

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
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> methodToInvoke, ICacheKey key, int expirationInSeconds, int currentUserLCID, bool isNullValueAllowedInCache) where T : class;

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <returns>The number of keys cleared from cache</returns>
        int ClearCache();

        /// <summary>
        /// Clear the cached information for a specific key (in the CurrentUICulture).
        /// </summary>
        /// <param name="key">The key.</param>
        void ClearCache(ICacheKey key);

        /// <summary>
        /// Clear the cached information for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="lcid">The language of the cache to clear.</param>
        void ClearCache(ICacheKey key, int lcid);

        /// <summary>
        /// Clear the cached information for a specific key.
        /// </summary>
        /// <param name="key">The key.</param>
        void ClearCache(string key);
    }
}
