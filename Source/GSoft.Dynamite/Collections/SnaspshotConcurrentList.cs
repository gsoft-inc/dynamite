using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Sharepoint.Collections
{
    /// <summary>
    /// A concurrent collection that will return a snapshot of itself when enumerating instead
    /// of locking for writing.
    /// </summary>
    /// <typeparam name="T">The type of object contained in the list.</typeparam>
    [Serializable]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is actually a list.")]
    public class SnaspshotConcurrentList<T> : ConcurrentList<T>
    {
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public override IEnumerator<T> GetEnumerator()
        {
            var copiedList = new List<T>();

            // use an enumerator to go over all items and copy them to a snapshot list
            using (var enumerator = base.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    copiedList.Add(enumerator.Current);
                }
            }

            return copiedList.GetEnumerator();
        }
    }
}
