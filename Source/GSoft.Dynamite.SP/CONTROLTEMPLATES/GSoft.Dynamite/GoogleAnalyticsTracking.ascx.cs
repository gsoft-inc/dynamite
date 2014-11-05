using System;
using System.Web.UI;
using Autofac;
using GSoft.Dynamite.Configuration;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    /// <summary>
    /// Google analytics user control
    /// </summary>
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
        /// Gets or sets a value indicating whether this instance is in display mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is in display mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsInDisplayMode { get; set; }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            SPWeb rootWeb = SPContext.Current.Site.RootWeb;
            using (var scope = DynamiteWspContainerProxy.BeginLifetimeScope(rootWeb))
            {
                var configuration = scope.Resolve<IConfiguration>();

                this.GoogleAnalyticsID = configuration.GetGoogleAnalyticsIdByMostNestedScope(rootWeb);
                this.IsInDisplayMode = SPContext.Current.FormContext.FormMode == SPControlMode.Display;
            }
        }
    }
}
