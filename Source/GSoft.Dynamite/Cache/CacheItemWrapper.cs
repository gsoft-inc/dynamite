namespace GSoft.Dynamite.Cache
{
    /// <summary>
    /// The cache item wrapper.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class CacheItemWrapper<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemWrapper{T}"/> class.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public CacheItemWrapper(T item)
        {
            this.Item = item;
        }

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        public T Item { get; set; }
    }
}
