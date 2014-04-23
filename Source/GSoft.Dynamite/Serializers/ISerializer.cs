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
    }
}