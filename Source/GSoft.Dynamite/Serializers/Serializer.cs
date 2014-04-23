namespace GSoft.Dynamite.Serializers
{
    using fastJSON;

    /// <summary>
    /// The serializer.
    /// </summary>
    public class Serializer : ISerializer
    {
        /// <summary>
        /// The serialize method using fastJSON in 3.5. ServiceStack.Text is not available on this version of the .Net Framework.
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
    }
}
