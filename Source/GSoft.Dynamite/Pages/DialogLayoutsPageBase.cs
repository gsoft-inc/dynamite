using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace GSoft.Dynamite.Pages
{
    /// <summary>
    /// Base page for dialog layout pages.
    /// </summary>
    public abstract class DialogLayoutsPageBase : LayoutsPageBase
    {
        /// <summary>
        /// The key cancel source
        /// </summary>
        private const string KeyCancelSource = "CancelSource";

        /// <summary>
        /// Gets a value indicating whether [is read only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is read only]; otherwise, <c>false</c>.
        /// </value>
        protected bool IsReadOnly
        {
            get
            {
                return this.Site != null && this.Site.ReadOnly;
            }
        }

        /// <summary>
        /// Gets or sets the redirection page URL.
        /// </summary>
        /// <value>
        /// The redirection page URL.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Dialog onClose redirect URL should be manipulated as string to avoid bad concatenation.")]
        protected string RedirectionPageUrl { get; set; }

        /// <summary>
        /// Gets a value indicating whether [is dialog].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is dialog]; otherwise, <c>false</c>.
        /// </value>
        protected bool IsDialog
        {
            get
            {
                return !string.IsNullOrEmpty(this.Request.QueryString["IsDlg"]);
            }
        }

        /// <summary>
        /// Represents that method that handles the <see cref="E:System.Web.UI.Control.Load" /> event of the page.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data.</param>
        /// <exception cref="System.IO.FileNotFoundException">File not found.</exception>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (this.Web.CurrentUser == null)
            {
                SPUtility.HandleAccessDenied(new UnauthorizedAccessException());
            }

            if (!IsLayoutFolderInTheWeb(this.Web.ServerRelativeUrl))
            {
                throw new FileNotFoundException();
            }
        }

        /// <summary>
        /// Cancels the button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void CancelButtonClick(object sender, EventArgs e)
        {
            var url = this.Context.Request.QueryString[KeyCancelSource];
            if (SPUtility.RedirectValidate(url, this.Context))
            {
                SPUtility.Redirect(url, SPRedirectFlags.Static, this.Context);
                return;
            }

            SPUtility.Redirect("settings.aspx", SPRedirectFlags.RelativeToLayoutsPage, this.Context);
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        protected void CloseDialog()
        {
            this.CloseDialog(1);
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        /// <param name="result">Result code to pass. Available codes are: -1 = invalid; 0 = cancel; 1 = OK</param>
        protected void CloseDialog(int result)
        {
            this.CloseDialog(result, this.RedirectionPageUrl);
        }

        /// <summary>
        /// Closes the dialog.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="returnValue">The return value.</param>
        protected void CloseDialog(int result, string returnValue)
        {
            if (this.IsDialog)
            {
                this.Response.Clear();
                this.Response.Write(string.Format(CultureInfo.InvariantCulture, "<script type=\"text/javascript\">window.frameElement.commonModalDialogClose({0}, {1});</script>", new object[] { result, string.IsNullOrEmpty(returnValue) ? "null" : string.Format(CultureInfo.InvariantCulture, "\"{0}\"", returnValue) }));
                this.Response.End();
            }
            else
            {
                this.Redirect();
            }
        }

        private static bool IsLayoutFolderInTheWeb(string strServerRelativeWebUrl)
        {
            var requestUrl = SPUtility.OriginalServerRelativeRequestUrl;
            requestUrl = SPHttpUtility.UrlPathDecode(requestUrl, false).ToUpper(CultureInfo.InvariantCulture);
            var index = requestUrl.IndexOf("/_layouts/", StringComparison.OrdinalIgnoreCase);
            return (index > 0 && strServerRelativeWebUrl.ToUpper(CultureInfo.InvariantCulture) == requestUrl.Substring(0, index)) || (index == 0 && strServerRelativeWebUrl == "/");
        }

        private void Redirect()
        {
            SPUtility.Redirect(this.RedirectionPageUrl ?? SPContext.Current.Web.Url, SPRedirectFlags.UseSource, this.Context);
        }
    }
}
