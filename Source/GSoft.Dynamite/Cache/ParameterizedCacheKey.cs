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
        private readonly bool discriminateBetweenGroups;

        /// <summary>
        /// Creates a new parameter-based cache key to cache the same items regardless of the current language.
        /// Will discriminate against the current users SharePoint Groups by adding them to the key as a prefix.
        /// </summary>
        /// <param name="keyForBothLanguages">The key prefix to share between both English and French content items</param>
        public ParameterizedCacheKey(string keyForBothLanguages)
            : this(keyForBothLanguages, true)
        {
        }

        /// <summary>
        /// Creates a new parameter-based cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="keyForBothLanguages">The key prefix to share between both English and French content items</param>
        /// <param name="discriminateBetweenGroups">if set to <c>true</c> add a prefix in the key with the groups the current user belongs to.</param>
        public ParameterizedCacheKey(string keyForBothLanguages, bool discriminateBetweenGroups)
            : this(keyForBothLanguages, keyForBothLanguages, discriminateBetweenGroups)
        {
        }

        /// <summary>
        /// Creates a new parameter-based cache key to cache different items depending on the current language.
        /// Will discriminate against the current users SharePoint Groups by adding them to the key as a prefix.
        /// </summary>
        /// <param name="englishKeyPrefix">English key prefix to put in front of the parameter</param>
        /// <param name="frenchKeyPrefix">French key prefix to put in front of the parameter</param>
        public ParameterizedCacheKey(string englishKeyPrefix, string frenchKeyPrefix)
            : this(englishKeyPrefix, frenchKeyPrefix, true)
        {
        }

        /// <summary>
        /// Creates a new parameter-based cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="englishKeyPrefix">English key prefix to put in front of the parameter</param>
        /// <param name="frenchKeyPrefix">French key prefix to put in front of the parameter</param>
        /// <param name="discriminateBetweenGroups">if set to <c>true</c> add a prefix in the key with the groups the current user belongs to.</param>
        public ParameterizedCacheKey(string englishKeyPrefix, string frenchKeyPrefix, bool discriminateBetweenGroups)
        {
            this.englishPrefix = englishKeyPrefix + "-";
            this.frenchPrefix = frenchKeyPrefix + "-";
            this.discriminateBetweenGroups = discriminateBetweenGroups;
        }

        /// <summary>
        /// Creates the parameter-specific cache key
        /// </summary>
        /// <param name="parameter">The cache key suffix for parameterization</param>
        /// <returns>A parameterized cache key</returns>
        public ICacheKey WithParameter(string parameter)
        {
            return new SimpleCacheKey(this.englishPrefix + parameter, this.frenchPrefix + parameter, this.discriminateBetweenGroups);
        }
    }
}
