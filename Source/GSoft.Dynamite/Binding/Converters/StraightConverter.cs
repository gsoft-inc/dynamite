using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Binding
{
    using System;

    /// <summary>
    /// A converter that simply returns the value.
    /// </summary>
    public class StraightConverter : IConverter
    {
        #region Fields

        /// <summary>
        /// The instance of the StraightConverter.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "This instance is immutable.")]
        public static readonly StraightConverter Instance = new StraightConverter();

        #endregion

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
            if (value == DBNull.Value || value == null)
            {
                return null;
            }

            if (arguments.PropertyType == typeof(bool))
            {
                // If the value comes from a datarow, it may in 0/1 format.
                // Otherwise, just let the straight conversion take place.
                if (value.ToString() == "0")
                {
                    return false;
                } 
                else if (value.ToString() == "1")
                {
                    return true;
                }
            }

            // Okay now we really do a straight conversion
            return value;
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
            return value;
        }

        #endregion
    }
}