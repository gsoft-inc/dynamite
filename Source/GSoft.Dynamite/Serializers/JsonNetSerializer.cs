namespace GSoft.Dynamite.Serializers
{
    using Newtonsoft.Json;

    /// <summary>
    /// The service stack serializer.
    /// </summary>
    public class JSonNetSerializer : ISerializer
    {
        // private readonly JavaScriptSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JSonNetSerializer"/> class.
        /// </summary>
        public JSonNetSerializer()
        {
            // this.serializer = new JsonN();
        }

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
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
