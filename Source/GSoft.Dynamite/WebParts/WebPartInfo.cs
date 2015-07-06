using System;
using System.Globalization;
using System.Web.UI.WebControls.WebParts;
using Newtonsoft.Json;

namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public WebPartInfo()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WebPartInfo" /> instance
        /// </summary>
        /// <param name="zoneName">The name of zone in which the web part should be instantiated</param>
        /// <param name="webPart">The WebPart object that should be instantiated</param>
        public WebPartInfo(string zoneName, WebPart webPart)
            : this(zoneName, webPart, 1)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WebPartInfo" /> instance
        /// </summary>
        /// <param name="zoneName">The name of zone in which the web part should be instantiated</param>
        /// <param name="webPart">The WebPart object that should be instantiated</param>
        /// <param name="zoneIndex">The index in which the web part will find itself in the zone.</param>
        public WebPartInfo(string zoneName, WebPart webPart, int zoneIndex)
        {
            this.WebPart = webPart;
            this.ZoneName = zoneName;
            this.ZoneIndex = zoneIndex;
        }

        /// <summary>
        /// Name of the WebPartZone to which to add the web part
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The index in which the web part will find itself in the zone.
        /// </summary>
        public int ZoneIndex { get; set; }

        /// <summary>
        /// The WebPart object that should be provisioned
        /// </summary>
        [JsonIgnore]
        public WebPart WebPart { get; set; }

        /// <summary>
        /// The culture of the webpart.
        /// </summary>
        public CultureInfo Culture { get; set; }
    }
}
