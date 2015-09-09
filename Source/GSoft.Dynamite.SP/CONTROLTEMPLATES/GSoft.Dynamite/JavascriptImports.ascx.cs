using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    /// <summary>
    /// User control to add the import of JavaScript libraries
    /// </summary>
    public partial class JavaScriptImports : UserControl
    {
        /// <summary>
        /// Event receiver of the page load event
        /// </summary>
        /// <param name="sender">Object who send the event</param>
        /// <param name="e">event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            MakeBrowserCacheSafeOrRemoveIfMissing(this.DynamiteCoreScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryPlaceholderShim);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryNoConflictScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockoutScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.MomentScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.UnderscoreScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.DynamiteCoreScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockoutBindingHandlersScriptLink);
            MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockoutExtensionsScriptLink);
        }

        private static void MakeBrowserCacheSafeOrRemoveIfMissing(ScriptLink scriptLink)
        {
            try
            {
#if DEBUG
                if (!scriptLink.Name.Contains("/Lib/"))
                {
                    // we want to be able to debug our JS files more easily
                    scriptLink.Name = scriptLink.Name.Replace(".min.js", ".js");
                }
#endif

                // These are optional module, so trying to build these browser-cache-safe URLs may explode if the modules are missing
                scriptLink.Name = SPUtility.MakeBrowserCacheSafeLayoutsUrl(scriptLink.Name, false);
            }
            catch (SPException)
            {
                // Script not found, remove from page
                scriptLink.Parent.Controls.Remove(scriptLink);
            }
        }
    }
}
