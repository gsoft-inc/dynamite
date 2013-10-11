using System;
using System.Collections;
using System.Collections.Generic;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// A comparer class that uses a delegate.
    /// </summary>
    /// <typeparam name="T">The type for the comparer.</typeparam>
    public class DelegateComparer<T> : IComparer<T>, IComparer
    {
        #region Fields

        private readonly Func<T, T, int> _comparerFunction;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateComparer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="comparerFunction">The comparer function.</param>
        public DelegateComparer(Func<T, T, int> comparerFunction)
        {
            if (comparerFunction == null)
            {
                throw new ArgumentNullException("comparerFunction");
            }

            this._comparerFunction = comparerFunction;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(T x, T y)
        {
            return this._comparerFunction(x, y);
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
        public int Compare(object x, object y)
        {
            return this.Compare((T)x, (T)y);
        }

        #endregion
    }
}
