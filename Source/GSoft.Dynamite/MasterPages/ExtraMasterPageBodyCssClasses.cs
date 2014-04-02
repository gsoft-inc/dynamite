using System;
using System.Globalization;
using System.Web;
using GSoft.Dynamite;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using GSoft.Dynamite.Security;

namespace GSoft.Dynamite.MasterPages
{
    /// <summary>
    /// Used to add CSS classes to the body of the document
    /// </summary>
    public class ExtraMasterPageBodyCssClasses : IExtraMasterPageBodyCssClasses
    {
        private SecurityHelper securityHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraMasterPageBodyCssClasses"/> class.
        /// </summary>
        /// <param name="securityHelper">The security helper.</param>
        public ExtraMasterPageBodyCssClasses(SecurityHelper securityHelper)
        {
            this.securityHelper = securityHelper;
        }

        /// <summary>
        /// Detects the user's culture, browser agent and current group/permissions and returns a string with their abbreviation
        /// </summary>
        /// <returns>Returns a combination of useful classes to do browser sniffing and other context-dependent rendering in our CSS.</returns>
        public string AllExtraCssClasses
        {
            get
            {
                return this.CultureDetection + this.BrowserDetection + this.PageModeDetection + this.PermissionsDetection;
            }
        }

        /// <summary>
        /// Gets the culture detection.
        /// </summary>
        /// <value>
        /// The culture detection.
        /// </value>
        public string CultureDetection
        {
            get
            {
                string extraClasses = string.Empty;

                // UI Language
                if (CultureInfo.CurrentUICulture.LCID == Language.English.Culture.LCID)
                {
                    extraClasses += "en ";
                }
                else
                {
                    extraClasses += "fr ";
                }

                return extraClasses;
            }
        }

        /// <summary>
        /// Gets the browser detection.
        /// </summary>
        /// <value>
        /// The browser detection.
        /// </value>
        public string BrowserDetection
        {
            get
            {
                string extraClasses = string.Empty;

                // Browser user agent detection
                if (HttpContext.Current.Request.UserAgent.Contains("Firefox"))
                {
                    extraClasses += "firefox ";
                }
                else if (HttpContext.Current.Request.UserAgent.Contains("Chrome")
                    || HttpContext.Current.Request.UserAgent.Contains("Safari")
                    || HttpContext.Current.Request.UserAgent.Contains("WebKit"))
                {
                    extraClasses += "webkit ";
                }
                else if (HttpContext.Current.Request.UserAgent.Contains("MSIE"))
                {
                    extraClasses += "ie ";

                    if (HttpContext.Current.Request.UserAgent.Contains("Trident/3.0"))
                    {
                        extraClasses += "ie7 ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("MSIE 7.0"))
                    {
                        // Compatibility View detection (i.e. IE7)
                        extraClasses += "ie7-docmode compat-view ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("Trident/4.0"))
                    {
                        extraClasses += "ie8 ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("MSIE 8.0"))
                    {
                        extraClasses += "ie8-docmode ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("Trident/5.0"))
                    {
                        extraClasses += "ie9 ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("MSIE 9.0"))
                    {
                        extraClasses += "ie9-docmode ";
                    }

                    if (HttpContext.Current.Request.UserAgent.Contains("Trident/6.0"))
                    {
                        extraClasses += "ie10 ";
                    }
                }

                return extraClasses;
            }
        }

        /// <summary>
        /// Gets the page mode detection.
        /// </summary>
        /// <value>
        /// The page mode detection.
        /// </value>
        public string PageModeDetection
        {
            get
            {
                var extraClasses = string.Empty;

                // Edit mode detection
                if (SPContext.Current.FormContext != null
                    && SPContext.Current.FormContext.FormMode == SPControlMode.Edit)
                {
                    extraClasses += "editmode ";
                }

                return extraClasses;
            }
        }

        /// <summary>
        /// Gets the permissions detection.
        /// </summary>
        /// <value>
        /// The permissions detection.
        /// </value>
        public string PermissionsDetection
        {
            get
            {
                try
                {
                    var extraClasses = string.Empty;

                    if (this.securityHelper.IsCurrentUserVisitor())
                    {
                        extraClasses += "visitor ";
                    }
                    
                    if (this.securityHelper.IsCurrentUserMember())
                    {
                        extraClasses += "member ";
                    }
                    
                    if (this.securityHelper.IsCurrentUserApprover())
                    {
                        extraClasses += "approver ";
                    }
                        
                    if (this.securityHelper.IsCurrentUserOwner()
                        || (SPContext.Current.Web != null
                            && SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.FullMask)))
                    {
                        extraClasses += "owner ";
                    }

                    return extraClasses;
                }
                catch (InvalidOperationException)
                {
                    // this exception can be thrown when a new ListItem is created. At the creation time the permission are not set yet so it cannot be check that's why the error is thrown.
                    return string.Empty;
                }
            }
        }
    }
}
