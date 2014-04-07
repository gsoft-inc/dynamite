using System;

namespace GSoft.Dynamite.Caching.Entities
{
    /// <summary>
    /// A serializable key value pair structure.
    /// </summary>
    /// <typeparam name="TK">Key type.</typeparam>
    /// <typeparam name="TV">Value type.</typeparam>
    [Serializable]
    [Obsolete]
    public struct CacheKeyValuePair<TK, TV>
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public TK Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TV Value { get; set; }
    }
}
