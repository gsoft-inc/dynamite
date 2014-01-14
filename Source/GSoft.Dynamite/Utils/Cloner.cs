using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// A utility class for creating clones.
    /// </summary>
    public static class Cloner
    {
        /// <summary>
        /// Performs a binary clone using a binary formatter.
        /// </summary>
        /// <typeparam name="T">The type of object being cloned.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The clone.</returns>
        public static T BinaryClone<T>(T source)
            where T : class
        {
            if (source == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                // override URI serialization since it does not work properly for Relative URLs (adds escape characters that were not there)
                var surrogates = new SurrogateSelector();
                surrogates.AddSurrogate(typeof(Uri), new StreamingContext(StreamingContextStates.All), new UriSerializationSurrogate());
                formatter.SurrogateSelector = surrogates;

                formatter.Serialize(ms, source);

                ms.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// Creates a clone by using XML serialization and deserialization.
        /// </summary>
        /// <typeparam name="T">The type of object being cloned.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns>The clone.</returns>
        public static T XmlClone<T>(T source)
            where T : class
        {
            if (source == null)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                var serializer = new XmlSerializer(typeof(T));

                serializer.Serialize(ms, source);

                ms.Seek(0, SeekOrigin.Begin);

                return (T)serializer.Deserialize(ms);
            }
        }

        private class UriSerializationSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                info.AddValue("__value", obj.ToString());
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var uri = info.GetString("__value");
                return new Uri(uri, UriKind.RelativeOrAbsolute);
            }
        }
    }
}
