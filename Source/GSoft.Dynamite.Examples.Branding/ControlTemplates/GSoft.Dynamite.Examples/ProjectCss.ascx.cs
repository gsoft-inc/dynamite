using System;
using System.Web.UI;
using GSoft.Dynamite.Examples.Utilities;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.Examples.Branding.ControlTemplates
{
    /// <summary>
    /// Includes the CSS registrations for the project's stylesheets
    /// </summary>
    public partial class ProjectCss : UserControl
    {
        /// <summary>
        /// Fires when the page loads
        /// </summary>
        /// <param name="sender">Originator of event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Css link elements need to be generated on the fly - rewriting their arguments from code-behind doesn't work.
            // The Css import sequence should be corev4.css -> GSoft.Dynamite.Examples.css -> GSoft.Dynamite.Examples.Custom.css.

            // Add a version string to layouts CSS file, otherwise a stale version will get cached in the clients' browsers
            string projectCssName = SPUtility.ConcatUrls(SPContext.Current.Web.ServerRelativeUrl, "/_layouts/GSoft.Dynamite.Examples/css/GSoft.Dynamite.Examples.css?v=" + VersionContext.CurrentVersionTag);

            CssRegistration css = new CssRegistration();
            css.ID = "ProjectCssRegistration";
            css.After = "corev4.css";
            css.Name = projectCssName;
            this.Controls.Add(css);

            CssRegistration customCss = new CssRegistration();
            customCss.ID = "ProjectCustomCssRegistration";
            customCss.After = projectCssName;

            // no need for a version string here - the Style Library won't unnecessarily cache the css file
            customCss.Name = SPUtility.ConcatUrls(SPContext.Current.Web.ServerRelativeUrl, "/Style Library/GSoft.Dynamite.Examples.Custom.css");
            this.Controls.Add(customCss);
        }
    }
}
