namespace GSoft.Dynamite.Serializers
{
    using ServiceStack.Text;

    /// <summary>
    /// The service stack serializer.
    /// </summary>
    public class ServiceStackSerializer : ISerializer
    {
        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="obj">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Serialize(object obj)
        {
            return JsonSerializer.SerializeToString(obj);
        }

        /// <summary>
        /// The deserialize method.
        /// </summary>
        /// <param name="json">
        /// The JSON to deserialize.
        /// </param>
        /// <typeparam name="T">
        /// The type to deserialize
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.DeserializeFromString<T>(json);
        }
    }
}
