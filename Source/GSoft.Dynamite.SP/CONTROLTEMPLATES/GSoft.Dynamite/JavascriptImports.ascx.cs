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
        /// Root folder URL format
        /// </summary>
        public const string ListRootFolderUrlFormat = "{0}/Forms/AllItems.aspx?RootFolder={1}";

        /// <summary>
        /// Event receiver of the page load event
        /// </summary>
        /// <param name="sender">Object who send the event</param>
        /// <param name="e">event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.CurrentWebUrlLiteral.Text = SPContext.Current.Web.Url;

            if (SPContext.Current.List != null)
            {
                var listUrl = SPContext.Current.List.RootFolder.ServerRelativeUrl;

                if (HttpContext.Current.Request.Url.AbsoluteUri.Contains("/Forms/AllItems.aspx?RootFolder="))
                {
                    // we're already in a folder, so open the parent folder
                    var rootFolderQueryStringArgument = HttpContext.Current.Request.QueryString["RootFolder"];

                    if (!string.IsNullOrEmpty(rootFolderQueryStringArgument))
                    {
                        var parentFolderUrlSubStringLength = rootFolderQueryStringArgument.Length - (rootFolderQueryStringArgument.Length - rootFolderQueryStringArgument.LastIndexOf("/", StringComparison.OrdinalIgnoreCase));
                        if (parentFolderUrlSubStringLength > 0)
                        {
                            var parentFolderUrl = rootFolderQueryStringArgument.Substring(0, parentFolderUrlSubStringLength);

                            if (parentFolderUrl.Contains(listUrl))
                            {
                                this.ParentFolderUrlLiteral.Text = string.Format(CultureInfo.InvariantCulture, ListRootFolderUrlFormat, listUrl, parentFolderUrl);
                            }
                        }
                    }
                }
                else if (SPContext.Current.File != null)
                {
                    // go to AllItems view for current item's folder
                    this.ParentFolderUrlLiteral.Text = string.Format(CultureInfo.InvariantCulture, ListRootFolderUrlFormat, listUrl, SPContext.Current.File.ParentFolder.ServerRelativeUrl);
                }
            }

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
