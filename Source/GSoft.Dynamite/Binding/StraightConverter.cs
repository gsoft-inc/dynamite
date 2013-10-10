using System.Diagnostics.CodeAnalysis;

namespace GSoft.Dynamite.Sharepoint2013.Binding
{
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