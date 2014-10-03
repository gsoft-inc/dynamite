using System;
using System.Globalization;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The conversion class for a User.
    /// </summary>
    public class UserValueCollectionConverter : SharePointListItemValueConverter
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
        public override object Convert(object value, SharePointListItemConversionArguments arguments)
        {
            UserValueCollection userValueCollection = null;
            var collection = value as SPFieldUserValueCollection;
            try
            {
                if (collection != null)
                {
                    var userValues = collection.Select(userValue => new UserValue(userValue.User)).ToList();

                    userValueCollection = new UserValueCollection(userValues);
                }
            }
            catch (ArgumentException)
            {
                // failed to read SPUser value, will return null
            }

            return userValueCollection;            
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, SharePointListItemConversionArguments arguments)
        {
            // Not implemented
            return null;
        }

        #endregion
    }
}