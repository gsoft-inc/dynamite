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
        /// </summary>
        /// <typeparam name="T">Generic type of the return value</typeparam>
        /// <param name="repoCamlQuery">Function to get values</param>
        /// <param name="key">Cache key</param>
        /// <param name="expirationInSeconds">Expiration of the cache in seconds</param>
        /// <returns>Return value of the function</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> repoCamlQuery, ICacheKey key, int expirationInSeconds) where T : class;

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
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Get is prettier than GetFromCacheOrInvoke.")]
        T Get<T>(Func<T> repoCamlQuery, ICacheKey key, int expirationInSeconds, int currentUserLCID) where T : class;

        /// <summary>
        /// Clear all cached information
        /// </summary>
        /// <returns>The number of keys cleared from cache</returns>
        int ClearCache();
    }
}
