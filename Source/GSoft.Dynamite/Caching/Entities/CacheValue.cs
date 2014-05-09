using System;

namespace GSoft.Dynamite.Caching.Entities
{
    /// <summary>
    /// Cache value wrapper object
    /// </summary>
    /// <typeparam name="T">The cached object type.</typeparam>
    [Serializable]
    [Obsolete]
    public class CacheValue<T> : ICacheValue<T>
    {
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheValue{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public CacheValue(T value)
        {
            this._value = value;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value 
        {
            get
            {
                return this._value;
            }
        }
    }
}
