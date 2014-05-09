namespace GSoft.Dynamite.Serializers
{
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
        /// <param name="json">
        /// The json.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/> The object type to cast to.
        /// </returns>
        T Deserialize<T>(string json);
    }
}