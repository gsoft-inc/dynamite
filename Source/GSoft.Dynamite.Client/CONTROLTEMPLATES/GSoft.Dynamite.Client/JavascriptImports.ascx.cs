using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace GSoft.Dynamite.Client.CONTROLTEMPLATES.GSoft.Dynamite.Client
{
    using System.Web;

    using global::GSoft.Dynamite.Logging;

    using Microsoft.SharePoint;

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
         }
    }
}
