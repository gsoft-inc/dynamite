// -----------------------------------------------------------------------
// <copyright file="ParameterizedCacheKey.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// Parameterized cache key
    /// </summary>
    public class ParameterizedCacheKey
    {
        private readonly string englishPrefix;
        private readonly string frenchPrefix;

        /// <summary>
        /// Creates a new parameter-based cache key to cache the same items regardless of the current language
        /// </summary>
        /// <param name="keyForBothLanguages">The key prefix to share between both English and French content items</param>
        public ParameterizedCacheKey(string keyForBothLanguages)
            : this(keyForBothLanguages, keyForBothLanguages)
        {
        }

        /// <summary>
        /// Creates a new parameter-based cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="englishKeyPrefix">English key prefix to put in front of the parameter</param>
        /// <param name="frenchKeyPrefix">French key prefix to put in front of the parameter</param>
        public ParameterizedCacheKey(string englishKeyPrefix, string frenchKeyPrefix)
        {
            this.englishPrefix = englishKeyPrefix + "-";
            this.frenchPrefix = frenchKeyPrefix + "-";
        }

        /// <summary>
        /// Creates the parameter-specific cache key
        /// </summary>
        /// <param name="parameter">The cache key suffix for parameterization</param>
        /// <returns>A parameterized cache key</returns>
        public ICacheKey WithParameter(string parameter)
        {
            return new SimpleCacheKey(this.englishPrefix + parameter, this.frenchPrefix + parameter);
        }
    }
}
