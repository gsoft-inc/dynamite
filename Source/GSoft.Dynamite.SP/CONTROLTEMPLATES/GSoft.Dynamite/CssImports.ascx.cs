using System;
using System.Web.UI;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    /// <summary>
    /// CSS Imports
    /// </summary>
    public partial class CssImports : UserControl
    {
        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Dynamite Core Registration
            var dynamiteCoreCss = new CssRegistration();
            dynamiteCoreCss.ID = "DynamiteCoreCssRegistration";
            dynamiteCoreCss.After = "corev4.css";
            dynamiteCoreCss.Name = SPUtility.ConcatUrls(SPContext.Current.Site.ServerRelativeUrl, SPUtility.MakeBrowserCacheSafeLayoutsUrl("GSoft.Dynamite/CSS/GSoft.Dynamite.Core.css", false));
            this.Controls.Add(dynamiteCoreCss);
        }
    }
}