using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace GSoft.Dynamite.Structures
{
    /// <summary>
    /// A resource value.
    /// </summary>
    [Obsolete("Initialize ResourceLocator with all filenames so we don't have to specify the file for each key")]
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "No need to override equality operator.")]
    public struct ResourceValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceValue"/> struct.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="key">The key.</param>
        public ResourceValue(string file, string key)
            : this()
        {
            this.File = file;
            this.Key = key;
        }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string File { get; private set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the key as resource expression.
        /// </summary>
        /// <value>
        /// The key as resource expression.
        /// </value>
        public string KeyAsResourceExpression
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "$Resources:{0};", this.Key);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "$Resources:{0},{1};", this.File, this.Key);
        }
    }
}
