namespace GSoft.Dynamite.Serializers
{
    using System.Web.Script.Serialization;

    /// <summary>
    /// The javascript serializer.
    /// </summary>
    public class JavascriptSerializer : ISerializer
    {
        private readonly JavaScriptSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="JavascriptSerializer"/> class.
        /// </summary>
        public JavascriptSerializer()
        {
            this.serializer = new JavaScriptSerializer();
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
            return this.serializer.Serialize(obj);
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
            return this.serializer.Deserialize<T>(json);
        }
    }
}
