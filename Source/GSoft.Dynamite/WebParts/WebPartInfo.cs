using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        private WebPart webpart;

        /// <summary>
        /// Initializes a new <see cref="WebPartInfo"/> instance
        /// </summary>
        /// <param name="name">The title of web part</param>
        /// <param name="zoneName">The name of zone in which the web part should be instantiated</param>
        public WebPartInfo(string name, string zoneName)
        {
            this.Name = name;
            this.ZoneName = zoneName;
        }

        /// <summary>
        /// Title of the web part
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the WebPartZone to which to add the web part
        /// </summary>
        public string ZoneName { get; set; }

        /// <summary>
        /// The WebPart object that should be provisioned
        /// </summary>
        public WebPart WebPart
        {
            get
            {
                return this.webpart;
            }

            set
            {
                this.webpart = value;

                // Update the title
                this.webpart.Title = this.Name;
            }    
        }
    }
}
