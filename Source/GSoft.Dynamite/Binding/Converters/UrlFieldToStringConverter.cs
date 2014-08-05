using System;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// URL field value to string (loses the description information)
    /// </summary>
    public class UrlFieldToStringConverter : IConverter
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
            if (value == DBNull.Value)
            {
                return null;
            }

            var urlValue = value as SPFieldUrlValue;
            if (urlValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    urlValue = new SPFieldUrlValue(stringValue);
                }
            }

            return urlValue != null ? urlValue.Url : null;
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
            var urlValue = value as string;
            return urlValue != null ? new SPFieldUrlValue { Url = urlValue, Description = urlValue } : null;
        }

        #endregion
    }
}
