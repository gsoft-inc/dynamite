using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.WebParts
{
    /// <summary>
    /// Definition of a WebPart
    /// </summary>
    public class WebPartInfo
    {
        private WebPart _webpart;

        public WebPartInfo(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Name of the WebPart
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The WebPartObject
        /// </summary>
        public WebPart WebPart
        {
            get
            {
                return _webpart;
            }
            set
            {
                this._webpart = value;

                // Update the title
                this._webpart.Title = this.Name;
            }    
        }
    }
}
