using System;
using System.Globalization;
using System.Web;
using System.Web.UI;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.CONTROLTEMPLATES.GSoft.Dynamite
{
    /// <summary>
    /// User Control to add the Parent Folder Link
    /// </summary>
    public partial class ParentFolder : UserControl
    {
        private const string ListRootSlug = "/Forms/AllItems.aspx?RootFolder=";

        private string ListRootFolderUrlFormat = "{0}" + ListRootSlug + "{1}";

        /// <summary>
        /// The Current Web absolute link
        /// </summary>
        public string CurrentWebAbsolutePath { get; set; }

        /// <summary>
        /// The Parent Folder server-relative link
        /// </summary>
        public string ParentFolderServerRelativePath { get; set; }

        /// <summary>
        /// The Parent Folder Label
        /// </summary>
        public string ParentFolderLabel { get; set; }

        /// <summary>
        /// Event receiver of the page load event
        /// </summary>
        /// <param name="sender">Object who send the event</param>
        /// <param name="e">event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            // TODO: Add resource
            this.ParentFolderLabel = CultureInfo.CurrentUICulture.LCID == Language.French.Culture.LCID ? "Dossier parent" : "Parent Folder";

            this.CurrentWebAbsolutePath = SPContext.Current.Web.Url;

            if (SPContext.Current.List != null)
            {
                var listUrl = SPContext.Current.List.RootFolder.ServerRelativeUrl;

                if (HttpContext.Current.Request.Url.AbsoluteUri.Contains(ListRootSlug))
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
                                this.ParentFolderServerRelativePath = string.Format(
                                    CultureInfo.InvariantCulture, 
                                    this.ListRootFolderUrlFormat, 
                                    listUrl, 
                                    parentFolderUrl);
                            }
                        }
                    }
                }
                else if (SPContext.Current.File != null)
                {
                    // go to AllItems view for current item's folder
                    this.ParentFolderServerRelativePath = string.Format(CultureInfo.InvariantCulture, this.ListRootFolderUrlFormat, listUrl, SPContext.Current.File.ParentFolder.ServerRelativeUrl);
                }
            }
        }
    }
}
