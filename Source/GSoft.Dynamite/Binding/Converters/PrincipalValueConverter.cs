using System.Globalization;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// The converter for principals.
    /// </summary>
    public class PrincipalValueConverter : IConverter
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
            var principal = value as SPPrincipal;
            return principal != null ? new PrincipalValue(principal) : null;
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
            var principal = value as PrincipalValue;
            return principal != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", principal.Id, (principal.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;
        }

        #endregion
    }
}