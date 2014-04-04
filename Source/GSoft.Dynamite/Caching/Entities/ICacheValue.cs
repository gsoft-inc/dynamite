namespace GSoft.Dynamite.Caching.Entities
{
    /// <summary>
    /// Cache value wrapper object interface
    /// </summary>
    /// <typeparam name="T">The cached object type.</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface ICacheValue<T>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        T Value { get; }
    }
}