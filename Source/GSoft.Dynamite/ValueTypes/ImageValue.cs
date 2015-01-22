using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// An image value entity.
    /// </summary>
    public class ImageValue
    {
        private string imageUrl;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValue"/> class.
        /// </summary>
        public ImageValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageValue"/> class.
        /// </summary>
        /// <param name="fieldImageValue">The field image value.</param>
        public ImageValue(ImageFieldValue fieldImageValue)
        {
            this.Alignment = fieldImageValue.Alignment;
            this.AlternateText = fieldImageValue.AlternateText;
            this.BorderWidth = fieldImageValue.BorderWidth;
            this.Height = fieldImageValue.Height;
            this.HorizontalSpacing = fieldImageValue.HorizontalSpacing;
            this.Hyperlink = fieldImageValue.Hyperlink;
            this.ImageUrl = fieldImageValue.ImageUrl;
            this.OpenHyperlinkInNewWindow = fieldImageValue.OpenHyperlinkInNewWindow;
            this.VerticalSpacing = fieldImageValue.VerticalSpacing;
            this.Width = fieldImageValue.Width;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the alignment
        /// </summary>
        public string Alignment { get; set; }
        
        /// <summary>
        /// Gets or sets alt text
        /// </summary>
        public string AlternateText { get; set; }
        
        /// <summary>
        /// Gets or sets border width
        /// </summary>
        public int BorderWidth { get; set; }
        
        /// <summary>
        /// Gets or sets height
        /// </summary>
        public int Height { get; set; }
        
        /// <summary>
        /// Gets or sets horizontal spacing
        /// </summary>
        public int HorizontalSpacing { get; set; }
        
        /// <summary>
        /// Gets or sets image hyperlink
        /// </summary>
        public string Hyperlink { get; set; }
        
        /// <summary>
        /// Gets or sets image url
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Meant for direct mapping from pre-validated ImageFieldValue in ListItem.")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Uri", Justification = "We just want to use the Uri constructors to trigger UriFormatExceptions. No need to use the resulting Uri objects.")]
        public string ImageUrl 
        { 
            get
            {
                return this.imageUrl;
            }

            set
            {
                bool worksAsAbsolute = false;
                bool worksAsRelative = false;

                try
                {
                    new Uri(value, UriKind.Absolute);
                    worksAsAbsolute = true;
                }
                catch (UriFormatException)
                {
                }

                if (!worksAsAbsolute)
                {
                    try
                    {
                        new Uri(value, UriKind.Relative);
                        worksAsRelative = true;
                    }
                    catch (UriFormatException)
                    {
                    }
                }

                var exception = new ArgumentException("Specific invalid ImageUrl value '{0}'. Please give a full absolute URL or a relative URL that starts with a forward slash.", value);
                if (!worksAsAbsolute && !worksAsRelative)
                {
                    // Bad Uri format, absolute or relative
                    throw exception;
                }
                else if (!worksAsAbsolute && worksAsRelative && !value.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    // Works as relative, but missing forward slash (essential for ImageFieldValue formatting)
                    throw exception;
                }

                this.imageUrl = value;
            }
        }
        
        /// <summary>
        /// Gets or sets whether hyperlink should open in new window
        /// </summary>
        public bool OpenHyperlinkInNewWindow { get; set; }
        
        /// <summary>
        /// Gets or sets vertical spacing
        /// </summary>
        public int VerticalSpacing { get; set; }

        /// <summary>
        /// Gets or sets width
        /// </summary>
        public int Width { get; set; }

        #endregion
    }
}