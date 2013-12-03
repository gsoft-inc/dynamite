using System;
using System.Web.UI;
using GSoft.Dynamite.Examples.Utilities;

namespace GSoft.Dynamite.Examples.Parts.ControlTemplates
{
    /// <summary>
    /// Includes the script links for the project's Javascript
    /// </summary>
    public partial class ProjectScripts : UserControl
    {
        /// <summary>
        /// Fires when the page loads
        /// </summary>
        /// <param name="sender">Originator of event</param>
        /// <param name="e">Event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Add version tag to JS link
            this.ProjectScriptLink.Name = this.ProjectScriptLink.Name + "?v=" + VersionContext.CurrentVersionTag;
        }
    }
}
