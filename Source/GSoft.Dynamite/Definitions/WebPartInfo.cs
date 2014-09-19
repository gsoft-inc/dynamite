using System.Web.UI.WebControls.WebParts;
using Microsoft.Office.Server.Search.WebControls;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        /// <summary>
        /// The WebPartObject
        /// </summary>
        public WebPart WebPart { get; set;}

        /// <summary>
        /// The name of the zone
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// Index of the WebPart in the zone
        /// </summary>
        public string ZoneIndex { get; set; }
    }
}
