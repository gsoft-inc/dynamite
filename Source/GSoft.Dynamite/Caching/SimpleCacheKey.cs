namespace GSoft.Dynamite.Caching
{
    /// <summary>
    /// Simple cache key
    /// </summary>
    public class SimpleCacheKey : ICacheKey
    {
        /// <summary>
        /// Creates a new simple cache key to cache the same items regardless of the current language
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="keyForBothLanguages">The key to share between both English and French content items</param>
        public SimpleCacheKey(string keyPrefix, string keyForBothLanguages)
            : this(keyPrefix, keyForBothLanguages, keyForBothLanguages)
        {
        }

        /// <summary>
        /// Creates a new simple cache key to cache different items depending on the current language
        /// </summary>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="englishKey">English key</param>
        /// <param name="frenchKey">French key</param>
        public SimpleCacheKey(string keyPrefix, string englishKey, string frenchKey)
        {
            this.Prefix = keyPrefix;
            this.InEnglish = this.Prefix + englishKey;
            this.InFrench = this.Prefix + frenchKey;
        }

        /// <summary>
        /// The prefix of the keep to identify the Laval keys in the config list.
        /// </summary>
        public string Prefix { get; private set; }

        /// <summary>
        /// Get english key
        /// </summary>
        public string InEnglish { get; private set; }

        /// <summary>
        /// Get french key
        /// </summary>
        public string InFrench { get; private set; }
    }
}
