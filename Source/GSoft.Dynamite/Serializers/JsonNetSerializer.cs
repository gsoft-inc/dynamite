namespace GSoft.Dynamite.Serializers
{
    using Newtonsoft.Json;

    /// <summary>
    /// The service stack serializer.
    /// </summary>
    public class JsonNetSerializer : ISerializer
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
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// The deserialize method.
        /// </summary>
        /// <param name="json">
        /// The json to deserialize.
        /// </param>
        /// <typeparam name="T">
        /// The type to deserialize
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
