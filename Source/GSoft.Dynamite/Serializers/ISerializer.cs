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
        /// <param name="valueToSerialize">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The serialized string
        /// </returns>
        string Serialize(object valueToSerialize);

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