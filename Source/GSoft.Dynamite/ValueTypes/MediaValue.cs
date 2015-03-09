using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Publishing.WebControls;

namespace GSoft.Dynamite.ValueTypes
{
    /// <summary>
    /// A rich media (publishing video) value entity.
    /// </summary>
    public class MediaValue
    {
        private string url = null;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaValue"/> class.
        /// </summary>
        public MediaValue()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaValue"/> class.
        /// </summary>
        /// <param name="fieldMediaValue">The SharePoint field Media value.</param>
        public MediaValue(MediaFieldValue fieldMediaValue)
        {
            this.Title = fieldMediaValue.Title;
            this.Url = fieldMediaValue.MediaSource;
            this.PreviewImageUrl = fieldMediaValue.PreviewImageSource;
            this.DisplayMode = fieldMediaValue.DisplayMode;
            this.XamlTemplateUrl = fieldMediaValue.TemplateSource;
            this.InlineHeight = fieldMediaValue.InlineHeight;
            this.InlineWidth = fieldMediaValue.InlineWidth;
            this.IsAutoPlay = fieldMediaValue.AutoPlay;
            this.IsLoop = fieldMediaValue.Loop;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets title of video/audio file
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the source URL of the video/audio file (MediaSource property).
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Value is a string.")]
        public string Url 
        {
            get
            {
                return this.url;
            }

            set
            {
                this.url = HttpUtility.UrlDecode(value);
            }
        }

        /// <summary>
        /// Gets or sets the source URL video/audio preview image.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Value is a string.")]
        public string PreviewImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the video/audio's display mode (Inline, Overlay or FullScreen)
        /// </summary>
        public MediaDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets the XAML document URL that will determine the media player's skin
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Value is a string.")]
        public string XamlTemplateUrl { get; set; }

        /// <summary>
        /// CSS height of the media player which displayed in page
        /// </summary>
        public string InlineHeight { get; set; }

        /// <summary>
        /// CSS width of the media player which displayed in page
        /// </summary>
        public string InlineWidth { get; set; }

        /// <summary>
        /// Gets or sets whether the media should start playing as soon as the page loads
        /// </summary>
        public bool IsAutoPlay { get; set; }

        /// <summary>
        /// Gets or sets whether the video/audio should replay automatically
        /// </summary>
        public bool IsLoop { get; set; }

        #endregion
    }
}