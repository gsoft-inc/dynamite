using GSoft.Dynamite.ValueTypes;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// A converter for the Lookup type.
    /// </summary>
    public class LookupValueConverter : IConverter
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
            var lookupValue = value as SPFieldLookupValue;
            if (lookupValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    lookupValue = new SPFieldLookupValue(stringValue);
                }
            }

            return lookupValue != null ? new LookupValue(lookupValue) : null;
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
            var lookup = value as LookupValue;

            return lookup != null ? new SPFieldLookupValue(lookup.Id, lookup.Value) : null;
        }

        #endregion
    }
}