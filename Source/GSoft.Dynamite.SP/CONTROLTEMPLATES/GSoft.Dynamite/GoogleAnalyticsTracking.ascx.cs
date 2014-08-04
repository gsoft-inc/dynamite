using System;
using System.Web.UI;
using Microsoft.SharePoint;
using Autofac;
using GSoft.Dynamite.Configuration;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    public partial class GoogleAnalyticsTracking : UserControl
    {
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
            var configuration = DynamiteWspContainerProxy.Current.Resolve<IConfiguration>();
            SPWeb web = SPContext.Current.Site.RootWeb;

            this.GoogleAnalyticsID = configuration.GetGoogleAnalyticsIdByMostNestedScope(web);
        }
    }
}
