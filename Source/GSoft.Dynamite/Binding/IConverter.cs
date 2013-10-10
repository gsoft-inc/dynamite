namespace GSoft.Dynamite.Sharepoint2013.Binding
{
    /// <summary>
    /// A conversion handler.
    /// </summary>
    public interface IConverter
    {
        #region Methods

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        object Convert(object value, ConversionArguments arguments);

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        object ConvertBack(object value, ConversionArguments arguments);

        #endregion
    }
}