using GSoft.Dynamite.Sharepoint.ValueTypes;

namespace GSoft.Dynamite.Sharepoint.Binding.Converters
{
    /// <summary>
    /// Conversion class for URL values.
    /// </summary>
    public class UrlValueConverter : IConverter
    {
        #region IConverter Members

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public object Convert(object value, ConversionArguments arguments)
        {
            var urlValue = value as SPFieldUrlValue;
            if (urlValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    urlValue = new SPFieldUrlValue(stringValue);
                }
            }

            return urlValue != null ? new UrlValue(urlValue) : null;
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public object ConvertBack(object value, ConversionArguments arguments)
        {
            var urlValue = value as UrlValue;
            return urlValue != null ? new SPFieldUrlValue { Url = urlValue.Url, Description = urlValue.Description } : null;
        }

        #endregion
    }
}