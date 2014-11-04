namespace GSoft.Dynamite.Serializers
{
    using System.Web.Script.Serialization;

    /// <summary>
    /// The JavaScript serializer.
    /// </summary>
    public class JavaScriptSerializer : ISerializer
    {
        private readonly JavaScriptSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptSerializer"/> class.
        /// </summary>
        public JavaScriptSerializer()
        {
            this.serializer = new JavaScriptSerializer();
        }

        /// <summary>
        /// The serialize.
        /// </summary>
        /// <param name="valueToSerialize">
        /// The object to serialize.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string Serialize(object valueToSerialize)
        {
            return this.serializer.Serialize(valueToSerialize);
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
            return this.serializer.Deserialize<T>(json);
        }
    }
}
