namespace GSoft.Dynamite.Serializers
{
    /// <summary>
    /// Interface to any serializer
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// The serialize method.
        /// </summary>
        /// <param name="obj">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The serialized string
        /// </returns>
        string Serialize(object obj);

        /// <summary>
        /// The deserialize.
        /// </summary>
        /// <param name="json">The JSON.</param>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <returns>
        /// The <see cref="T"/> The object type to cast to.
        /// </returns>
        T Deserialize<T>(string json);
    }
}