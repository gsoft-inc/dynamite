using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;

namespace GSoft.Dynamite.Sharepoint.Collections
{
    /// <summary>
    /// A simple implementation of a ConcurrentDictionary using a ReaderWriter lock.
    /// </summary>
    /// <typeparam name="TKey">The type for the key.</typeparam>
    /// <typeparam name="TValue">The type for the value.</typeparam>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true, MayLeakOnAbort = true)]
    public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
    {
        #region Fields

        private readonly IDictionary<TKey, TValue> _underlyingDictionary;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        public ConcurrentDictionary()
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public ConcurrentDictionary(IDictionary<TKey, TValue> dictionary)
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="comparer">The comparer.</param>
        public ConcurrentDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) 
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public ConcurrentDictionary(IEqualityComparer<TKey> comparer) 
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ConcurrentDictionary(int capacity) 
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="comparer">The comparer.</param>
        public ConcurrentDictionary(int capacity, IEqualityComparer<TKey> comparer) 
        {
            this._underlyingDictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return this.ReadLocked(() => this._underlyingDictionary.Keys);
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        public ICollection<TValue> Values
        {
            get
            {
                return this.ReadLocked(() => this._underlyingDictionary.Values);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get
            {
                return this.ReadLocked(() => this._underlyingDictionary.Count);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return this.ReadLocked(() => this._underlyingDictionary.IsReadOnly);
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>  
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
        /// The property is retrieved and <paramref name="key"/> is not found.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public TValue this[TKey key]
        {
            get
            {
                return this.ReadLocked(() => this._underlyingDictionary[key]);
            }

            set
            {
                this.WriteLocked(() => this._underlyingDictionary[key] = value);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        ///   </exception>
        /// <exception cref="T:System.ArgumentException">
        /// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            this.WriteLocked(() => this._underlyingDictionary.Add(key, value));
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.WriteLocked(() => this._underlyingDictionary.Add(item));
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        public bool ContainsKey(TKey key)
        {
            return this.ReadLocked(() => this._underlyingDictionary.ContainsKey(key));
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
        /// </exception>
        public bool Remove(TKey key)
        {
            return this.WriteLocked(() => this._underlyingDictionary.Remove(key));
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
        /// </exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var innerValue = default(TValue);

            var res = this.ReadLocked(() => this._underlyingDictionary.TryGetValue(key, out innerValue));

            value = innerValue;
            return res;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public void Clear()
        {
            this.WriteLocked(() => this._underlyingDictionary.Clear());
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.ReadLocked(() => this._underlyingDictionary.Contains(item));
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.ReadLocked(() => this._underlyingDictionary.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.WriteLocked(() => this._underlyingDictionary.Remove(item));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.ReadLocked(() => this._underlyingDictionary.ToList().GetEnumerator());
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool managed)
        {
            this._lock.Dispose();
        }

        private void ReadLocked(Action readExpression)
        {
            this._lock.EnterReadLock();
            try
            {
                readExpression();
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        private T ReadLocked<T>(Func<T> readExpression)
        {
            this._lock.EnterReadLock();
            try
            {
                return readExpression();
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        private void WriteLocked(Action writeExpression)
        {
            this._lock.EnterWriteLock();
            try
            {
                writeExpression();
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        private T WriteLocked<T>(Func<T> writeExpression)
        {
            this._lock.EnterWriteLock();
            try
            {
                return writeExpression();
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        #endregion
    }
}
