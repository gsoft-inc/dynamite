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
        /// Test content database name
        /// </summary>
        public const string TestContentDatabaseName = "SP2013_Content_DynamiteIntegrationTest";

        /// <summary>
        /// Creates a temporary site collection on the default port 80 web application, with the default
        /// test site collection host name as a blank site.
        /// </summary>
        public SiteTestScope()
        {
            // Team site by default
            this.ManagedPathCreated = string.Empty;
            this.InitializeSite(DefaultSiteCollectionHostName, SPWebTemplate.WebTemplateSTS + "#1", (uint)1033);
        }

        /// <summary>
        /// Creates a temporary site collection on the local Farm's default port 80 web application
        /// </summary>
        /// <param name="siteCollectionHostName">Host name for the site collection on the default port 80 web application</param>
        /// <param name="templateName">Web template string identifier (see http://www.eblogin.com/eblogin/post/2011/04/13/sp-createSubSite.aspx#.VFz2LPnF8Ws) for full list.</param>
        public SiteTestScope(string siteCollectionHostName, string templateName)
        {
            this.ManagedPathCreated = string.Empty;
            this.InitializeSite(siteCollectionHostName, templateName, (uint)1033);
        }

        /// <summary>
        /// Creates a temporary site collection on the local Farm's default port 80 web application
        /// </summary>
        /// <param name="siteCollectionHostName">Host name for the site collection on the default port 80 web application</param>
        /// <param name="templateName">Web template string identifier (see http://www.eblogin.com/eblogin/post/2011/04/13/sp-createSubSite.aspx#.VFz2LPnF8Ws) for full list.</param>
        /// <param name="lcid">The site language</param>
        public SiteTestScope(string siteCollectionHostName, string templateName, uint lcid)
        {
            this.ManagedPathCreated = string.Empty;
            this.InitializeSite(siteCollectionHostName, templateName, lcid);
        }

        /// <summary>
        /// Creates a temporary site collection (default), at the specified managed path
        /// </summary>
        /// <param name="managedPath">Path to be added as an explicit managed path for this Web App</param>
        public SiteTestScope(string managedPath)
        {
            this.ManagedPathCreated = string.Empty;
            this.InitializeSiteAtManagedPath(managedPath);
        }

        /// <summary>
        /// The temporary test site collection
        /// </summary>
        public SPSite SiteCollection { get; private set; }

        /// <summary>
        /// If managedPath is not empty when calling Dispose, it will remove the managed path from the prefixes collection
        /// </summary>
        public string ManagedPathCreated { get; private set; } 

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
        /// Creates a new temporary team site. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <param name="lcid">Language of the site collection to create</param>
        /// <returns>A brand new team site test scope</returns>
        public static SiteTestScope TeamSite(int lcid)
        {
            return new SiteTestScope(DefaultSiteCollectionHostName, "STS#0", (uint)lcid);
        }

        /// <summary>
        /// Creates a new temporary blank site. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <param name="lcid">Language of the site collection to create</param>
        /// <returns>A brand new team site test scope</returns>
        public static SiteTestScope BlankSite(int lcid)
        {
            return new SiteTestScope(DefaultSiteCollectionHostName, "STS#1", (uint)lcid);
        }

        /// <summary>
        /// Creates a new temporary team site at the specified managed path. Please dispose of this instance once you are done with it.
        /// </summary>
        /// <param name="managedPath">The managed path</param>
        /// <returns>A brand new blank site at a managed path test scope</returns>
        public static SiteTestScope ManagedPathSite(string managedPath)
        {
            return new SiteTestScope(managedPath);
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

            // Delete the test managed path if one was added
            if (this.ManagedPathCreated != string.Empty)
            {
                this.SiteCollection.WebApplication.Prefixes.Delete(this.ManagedPathCreated);
            }

            this.SiteCollection.Dispose();
            this.SiteCollection = null;
        }

        private void InitializeSite(string hostName, string templateName, uint lcid)
        {
            var defaultPortWebApp = this.GetDefaultPortWebApp();
            SPContentDatabase testContentDatabase = this.EnsureTestContentDatabase(defaultPortWebApp);

            SPSiteCollection sites = testContentDatabase.Sites;

            SPSite existingSite = testContentDatabase.Sites.FirstOrDefault(site => site.Url == hostName);

            if (existingSite != null)
            {
                existingSite.Delete();
                existingSite.Dispose();

                // Refresh Sites collection
                sites = testContentDatabase.Sites;
            }

            SPSite newSite = sites.Add(
                hostName, 
                "Dynamite Test", 
                "Integration test temporary site", 
                lcid, 
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

        private void InitializeSiteAtManagedPath(string managedPath)
        {
            var defaultPortWebApp = this.GetDefaultPortWebApp(); 
            SPContentDatabase testContentDatabase = this.EnsureTestContentDatabase(defaultPortWebApp);

            var prefixCollection = defaultPortWebApp.Prefixes;

            if (!prefixCollection.Contains(managedPath))
            {
                // The hostname-site collection's prefix may still exist
                // if a test run was interrupted previously.
                prefixCollection.Add(managedPath, SPPrefixType.ExplicitInclusion);                
            }

            // Flag so dispose will remove the managed path when the site collection is deleted
            this.ManagedPathCreated = managedPath;

            var siteUrl = defaultPortWebApp.GetResponseUri(SPUrlZone.Default) + managedPath;

            SPSiteCollection sites = testContentDatabase.Sites;
            SPSite existingSite = sites.FirstOrDefault(site => site.Url == siteUrl);

            if (existingSite != null)
            {
                existingSite.Delete();
                existingSite.Dispose();

                // Refresh Sites collection
                sites = testContentDatabase.Sites;
            }

            var newSite = sites.Add(siteUrl, Environment.UserName, "test@test.com");

            this.SiteCollection = newSite;
        }

        private SPContentDatabase EnsureTestContentDatabase(SPWebApplication defaultPortWebApp)
        {
            SPContentDatabaseCollection databaseCollection = defaultPortWebApp.ContentDatabases;

            SPContentDatabase defaultWebAppContentDatabase = defaultPortWebApp.ContentDatabases[0];
            SPContentDatabase testContentDb = databaseCollection.Cast<SPContentDatabase>().FirstOrDefault(db => db.Name == TestContentDatabaseName);

            if (testContentDb != null)
            {
                // DB already exists, we gotta figure out if it's getting bloated or not.
                // A typical Content Database will weigh in at a almost 160MB right at the start. 
                // After creating a couple of hundred site collection, even if we  delete them 
                // everytime, some space gets wasted and the disk size required for a backup 
                // grows to more than 200MB.
                const ulong MaxNumberOfMb = 200;
                if (testContentDb.DiskSizeRequired > MaxNumberOfMb * 1024 * 1024)
                {
                    // Repeated site collection recreation has left behind traces
                    // in the content database, time to delete it and start fresh
                    foreach (SPSite site in testContentDb.Sites)
                    {
                        // Delete any straggling site collection (and hope to god no one has 
                        // piggybacked onto our content database with an important site collection
                        // of their own - their loss, anyway)
                        site.Delete();
                    }

                    testContentDb.Status = SPObjectStatus.Offline;
                    testContentDb.Unprovision();
                    databaseCollection.Delete(testContentDb.Id);
                    testContentDb = null;
                }
                else
                {
                    // We haven't hit the size limit yet, let's re-use the same content database
                    // ...
                }
            }

            if (testContentDb == null)
            {
                // DB doesn't exist yet (or has just been deleted): we should (re)create it.
                testContentDb = databaseCollection.Add(
                    defaultWebAppContentDatabase.Server,
                    TestContentDatabaseName,
                    defaultWebAppContentDatabase.Username,
                    defaultWebAppContentDatabase.Password,
                    2000,
                    5000,
                    0);       // Status = 0 means the database is READY (instead of OFFLINE, which is Status = 1)
            }

            return testContentDb;
        }

        private SPWebApplication GetDefaultPortWebApp()
        {
            SPFarm farm = SPFarm.Local;
            SPWebService service = farm.Services.GetValue<SPWebService>(string.Empty);

            SPWebApplication defaultPortWebApp = service.WebApplications.FirstOrDefault(wa => wa.GetResponseUri(SPUrlZone.Default).Port == 80);

            if (defaultPortWebApp == null)
            {
                throw new InvalidOperationException("Failed to initialize temporary test SPSite! Can't find default port 80 web application on which to create site.");
            }

            return defaultPortWebApp;
        }
    }
}
