using GSoft.Dynamite.Sharepoint2013.ValueTypes;
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.Sharepoint2013.Binding.Converters
{
    /// <summary>
    /// Conversion class for image values.
    /// </summary>
    public class ImageValueConverter : IConverter
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
            var imageValue = value as ImageFieldValue;
            if (imageValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    imageValue = new ImageFieldValue(stringValue);
                }
            }

            return imageValue != null ? new ImageValue(imageValue) : null;
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
            var imageValue = value as ImageValue;

            ImageFieldValue backConverted = null;

            if (imageValue != null)
            {
                backConverted = new ImageFieldValue()
                {
                    Alignment = imageValue.Alignment,
                    AlternateText = imageValue.AlternateText,
                    BorderWidth = imageValue.BorderWidth,
                    Height = imageValue.Height,
                    HorizontalSpacing = imageValue.HorizontalSpacing,
                    Hyperlink = imageValue.Hyperlink,
                    ImageUrl = imageValue.ImageUrl,
                    OpenHyperlinkInNewWindow = imageValue.OpenHyperlinkInNewWindow,
                    VerticalSpacing = imageValue.VerticalSpacing,
                    Width = imageValue.Width,
                };
            }

            return backConverted;
        }

        #endregion
    }
}