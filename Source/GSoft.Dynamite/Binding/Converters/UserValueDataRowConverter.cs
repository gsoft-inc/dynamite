using System.Globalization;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    using System;

    /// <summary>
    /// The conversion class for a User.
    /// </summary>
    public class UserValueDataRowConverter : DataRowValueConverter
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
        public override object Convert(object value, DataRowConversionArguments arguments)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            UserValue userValue = null;
            var sharepointUserValue = new SPFieldUserValue(arguments.Web, value as string);
            var principal = sharepointUserValue.User;
            userValue = principal != null ? new UserValue(principal) : null;

            return userValue;            
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, DataRowConversionArguments arguments)
        {
            var principal = value as UserValue;
            return principal != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", principal.Id, (principal.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;
        }

        #endregion
    }
}