namespace GSoft.Dynamite.Serializers
{
    using fastJSON;

    /// <summary>
    /// The serializer.
    /// </summary>
    public class FastJsonSerializer : ISerializer
    {
        /// <summary>
        /// The serialize method using fastJSON in 3.5.
        /// </summary>
        /// <param name="obj">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The serialized string
        /// </returns>
        public string Serialize(object obj)
        {
            return JSON.ToJSON(obj, new JSONParameters() { UseEscapedUnicode = true });
        }

        /// <summary>
        /// The deserialize.
        /// </summary>
        /// <param name="json">
        /// The json.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>The object type to cast to.
        /// </returns>
        public T Deserialize<T>(string json)
        {
            return JSON.ToObject<T>(json);
        }
    }
}
