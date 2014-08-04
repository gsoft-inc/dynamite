using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    public partial class GoogleAnalyticsTracking : UserControl
    {
        private const string PropertyKey = "GSOFT_DYNAMITE_GOOGLE_ANALYTICS_TRACKING_ID";
        /// <summary>
        /// Gets or sets the google analytics identifier.
        /// </summary>
        /// <value>
        /// The google analytics identifier.
        /// </value>
        public string GoogleAnalyticsID { get; set; }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            SPWeb web = SPContext.Current.Site.RootWeb;
            var allproperties = web.AllProperties;

            if (allproperties.ContainsKey(PropertyKey))
            {
                GoogleAnalyticsID = allproperties[PropertyKey].ToString();
            }
        }
    }
}
