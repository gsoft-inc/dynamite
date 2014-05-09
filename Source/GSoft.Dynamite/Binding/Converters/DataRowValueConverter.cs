using System;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// A base class for SharePoint list item value conversions.
    /// </summary>
    public abstract class DataRowValueConverter : IConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        object IConverter.Convert(object value, ConversionArguments arguments)
        {
            var dataRowArguments = arguments as DataRowConversionArguments;
            if (dataRowArguments == null)
            {
                throw new ArgumentException("A DataRowValueConverter can only be used with a Data Row.");
            }

            return this.Convert(value, dataRowArguments);
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        object IConverter.ConvertBack(object value, ConversionArguments arguments)
        {
            var sharePointArguments = arguments as DataRowConversionArguments;
            if (sharePointArguments == null)
            {
                throw new ArgumentException("A SharePointValueConverter can only be used with a SharePoint list item.");
            }

            return this.ConvertBack(value, sharePointArguments);
        }

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public abstract object Convert(object value, DataRowConversionArguments arguments);

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public abstract object ConvertBack(object value, DataRowConversionArguments arguments);
    }
}
