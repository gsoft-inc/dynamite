using System;

namespace GSoft.Dynamite.Sharepoint2013.Binding.Converters
{
    /// <summary>
    /// A base class for SharePoint list item value conversions.
    /// </summary>
    public abstract class SharePointListItemValueConverter : IConverter
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
            var sharePointArguments = arguments as SharePointListItemConversionArguments;
            if (sharePointArguments == null)
            {
                throw new ArgumentException("A SharePointValueConverter can only be used with a SharePoint list item.");
            }

            return this.Convert(value, sharePointArguments);
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
            var sharePointArguments = arguments as SharePointListItemConversionArguments;
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
        public abstract object Convert(object value, SharePointListItemConversionArguments arguments);

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public abstract object ConvertBack(object value, SharePointListItemConversionArguments arguments);
    }
}
