using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A URL value entity.
    /// </summary>
    public class UrlValue
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValue"/> class.
        /// </summary>
        public UrlValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlValue"/> class.
        /// </summary>
        /// <param name="fieldUrlValue">The field URL value.</param>
        public UrlValue(SPFieldUrlValue fieldUrlValue)
        {
            this.Url = fieldUrlValue.Url;
            this.Description = fieldUrlValue.Description;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Value is a string.")]
        public string Url { get; set; }

        #endregion
    }
}