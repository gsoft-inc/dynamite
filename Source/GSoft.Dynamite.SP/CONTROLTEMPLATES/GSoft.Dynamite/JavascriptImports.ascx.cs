using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    public partial class JavascriptImports : UserControl
    {
        /// <summary>
        /// Root folder URL format
        /// </summary>
        public const string ListRootFolderUrlFormat = "{0}/Forms/AllItems.aspx?RootFolder={1}";

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
                        var parentFolderUrlSubStringLength = rootFolderQueryStringArgument.Length - (rootFolderQueryStringArgument.Length - rootFolderQueryStringArgument.LastIndexOf("/"));
                        if (parentFolderUrlSubStringLength > 0)
                        {
                            var parentFolderUrl = rootFolderQueryStringArgument.Substring(0, parentFolderUrlSubStringLength);

                            if (parentFolderUrl.Contains(listUrl))
                            {
                                this.ParentFolderUrlLiteral.Text = string.Format(ListRootFolderUrlFormat, listUrl, parentFolderUrl);
                            }
                        }
                    }
                }
                else if (SPContext.Current.File != null)
                {
                    // go to AllItems view for current item's folder
                    this.ParentFolderUrlLiteral.Text = string.Format(ListRootFolderUrlFormat, listUrl, SPContext.Current.File.ParentFolder.ServerRelativeUrl);
                }
            }

            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.DynamiteCoreScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryPlaceHolderShim);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.JqueryNoConflictScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockOutScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.MomentScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.UnderscoreScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.DynamiteCoreScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockoutBindingHandlersScriptLink);
            this.MakeBrowserCacheSafeOrRemoveIfMissing(this.KnockoutExtensionsScriptLink);
        }

        private void MakeBrowserCacheSafeOrRemoveIfMissing(ScriptLink scriptLink)
        {
            try
            {
                // These are optional module, so trying to build these browser-cache-safe URLs may explode if the modules are missing
                scriptLink.Name = SPUtility.MakeBrowserCacheSafeLayoutsUrl(scriptLink.Name, false);
            }
            catch (SPException ex)
            {
                // Script not found, remove from page
                scriptLink.Parent.Controls.Remove(scriptLink);

                // Current.Resolve<ILogger>().Info("Failed to find script in layouts folder. Removing script link from page: " + scriptLink.Name);
            }
        }
    }
}
