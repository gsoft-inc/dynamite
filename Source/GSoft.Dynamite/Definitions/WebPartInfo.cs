using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        /// <summary>
        /// Name of the WebPart
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The WebPartObject
        /// </summary>
        public WebPart WebPart { get; set; }
    }
}
