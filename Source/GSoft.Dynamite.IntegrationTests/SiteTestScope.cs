using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace GSoft.Dynamite.IntegrationTests
{
    /// <summary>
    /// Creates and tears down a full SharePoint site collection for
    /// intergration test isolation.
    /// </summary>
    public class SiteTestScope : IDisposable
    {
        /// <summary>
        /// Default test site collection hostname
        /// </summary>
        public const string DefaultSiteCollectionHostName = "http://dynamite.sharepoint.test";

        /// <summary>
        /// Creates a temporary site collection on the default port 80 web application, with the default
        /// test site collection host name as a blank site.
        /// </summary>
        public SiteTestScope()
        {
            // Team site by default
            this.InitializeSite(DefaultSiteCollectionHostName, SPWebTemplate.WebTemplateSTS + "#1");
        }

        /// <summary>
        /// Creates a temporary site collection on the local Farm's default port 80 web application
        /// </summary>
        /// <param name="siteCollectionHostName">Host name for the site collection on the default port 80 web application</param>
        /// <param name="templateName">Web template string identifier (see http://www.eblogin.com/eblogin/post/2011/04/13/sp-createSubSite.aspx#.VFz2LPnF8Ws) for full list.</param>
        public SiteTestScope(string siteCollectionHostName, string templateName)
        {
            this.InitializeSite(siteCollectionHostName, templateName);
        }

        /// <summary>
        /// The temporary test site collection
        /// </summary>
        public SPSite SiteCollection { get; private set; }

        /// <summary>
        /// Creates a new temporary team site. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <returns>A brand new team site test scope</returns>
        public static SiteTestScope TeamSite()
        {
            return new SiteTestScope(DefaultSiteCollectionHostName, "STS#0");
        }

        /// <summary>
        /// Creates a new temporary blank site. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <returns>A brand new team site test scope</returns>
        public static SiteTestScope BlankSite()
        {
            return new SiteTestScope();
        }

        /// <summary>
        /// Creates a new temporary publishing site. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <returns>A brand new team site test scope</returns>
        public static SiteTestScope PublishingSite()
        {
            return new SiteTestScope(DefaultSiteCollectionHostName, "BLANKINTERNET#2");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="managed"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool managed)
        {
            // Completely wipe out the temp site collection
            this.SiteCollection.Delete();
            this.SiteCollection.Dispose();
            this.SiteCollection = null;
        }

        private void InitializeSite(string hostName, string templateName)
        {
            SPFarm farm = SPFarm.Local;
            SPWebService service = farm.Services.GetValue<SPWebService>(string.Empty);

            SPWebApplication defaultPortWebApp = service.WebApplications.FirstOrDefault(wa => wa.GetResponseUri(SPUrlZone.Default).Port == 80);

            if (defaultPortWebApp == null)
            {
                throw new InvalidOperationException("Failed to initialize temporary test SPSite! Can't find default port 80 web application on which to create site.");
            }

            SPSiteCollection sites = defaultPortWebApp.Sites;

            SPSite existingSite = defaultPortWebApp.Sites.FirstOrDefault(site => site.Url == hostName);

            if (existingSite != null)
            {
                existingSite.Delete();
                existingSite.Dispose();

                // Refresh Sites collection
                sites = defaultPortWebApp.Sites;
            }

            SPSite newSite = sites.Add(
                hostName, 
                "Dynamite Test", 
                "Integration test temporary site", 
                1033, 
                templateName, 
                Environment.UserName, 
                "Dynamite Test Agent", 
                "test@test.com", 
                Environment.UserName, 
                "Dynamite Test Agent", 
                "test@test.com", 
                true);

            this.SiteCollection = newSite;
        }
    }
}
