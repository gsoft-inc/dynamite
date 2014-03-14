using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSoft.Dynamite.Collections
{
    /// <summary>
    /// The collection helper.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// The add range extension.
        /// </summary>
        /// <param name="destination">
        /// The destination collection.
        /// </param>
        /// <param name="source">
        /// The source enumerable.
        /// </param>
        /// <typeparam name="T"> T is type of entity inside collection
        /// </typeparam>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }
    }
}
