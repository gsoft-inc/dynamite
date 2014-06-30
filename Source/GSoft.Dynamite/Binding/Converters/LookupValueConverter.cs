using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.Converters
{
    using System;
    using System.Diagnostics;

    using GSoft.Dynamite.Logging;

    /// <summary>
    /// A converter for the Lookup type.
    /// </summary>
    public class LookupValueConverter : IConverter
    {
        private readonly ILogger logger;

        /// <summary>Initializes a new instance of the <see cref="LookupValueConverter"/> class.</summary>
        /// <param name="logger">The logger.</param>
        public LookupValueConverter(ILogger logger)
        {
            this.logger = logger;
        }

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

            if (value == DBNull.Value)
            {
                return null;
            }

            if (lookupValue != null)
            {
                return new LookupValue(lookupValue);
            }

            var stringValue = value as string;

            // Check if ;# split key is in string, if so, create SPFieldLookupValue with Id
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            if (stringValue.Contains(";#"))
            {
                var values = stringValue.Split(new[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                lookupValue = new SPFieldLookupValue(System.Convert.ToInt32(values[0]), values[1]);
            }
            else
            {
                this.logger.Info(string.Format("About to create a new SPFieldLookupValue with string {0}  StackTrace: ", stringValue, Environment.StackTrace));
              
                lookupValue = new SPFieldLookupValue(stringValue);    
            }

            return new LookupValue(lookupValue);
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