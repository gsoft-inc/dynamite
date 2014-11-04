using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Catalogs;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Catalogs
{
    /// <summary>
    /// Helper class for Cross Site Publishing operations
    /// </summary>
    public class CatalogHelper : ICatalogHelper
    {
        private readonly ILogger logger;
        private readonly IListHelper listHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="listHelper">The list helper</param>
        public CatalogHelper(ILogger logger, IListHelper listHelper)
        {
            this.logger = logger;
            this.listHelper = listHelper;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableManagedProperties">List of internal field names that are available through the catalog.</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableManagedProperties)
        {
            return this.SetListAsCatalog(list, availableManagedProperties, false);
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableManagedProperties">List of internal field names that are available through the catalog.</param>
        /// <param name="activateAnonymousAccess">if set to <c>true</c> [activate anonymous access].</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableManagedProperties, bool activateAnonymousAccess)
        {
            this.logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

            // Add properties for catalog publishing on the root folder
            list.IndexedRootFolderPropertyKeys.Add("PublishingCatalogSettings");
            list.IndexedRootFolderPropertyKeys.Add("IsPublishingCatalog");

            if (activateAnonymousAccess)
            {
                // Allow anonymous access on the parentWeb
                list.ParentWeb.FirstUniqueAncestorWeb.AnonymousPermMask64 |= SPBasePermissions.AnonymousSearchAccessWebLists;

                // Break list inheritance for anonymous access
                list.BreakRoleInheritance(true, false);

                // Allow anonymous access on the list
                list.AnonymousPermMask64 |= SPBasePermissions.AnonymousSearchAccessList;
            }

            var fieldList = new Collection<string>();

            // For fields name, you need to pass the internal name of the column directly followed by "OWSTEXT"
            foreach (var availableField in availableManagedProperties)
            {
                fieldList.Add("\"" + availableField + "\"");
            }

            var friendlyUrlFieldsProperty = string.Join(",", fieldList.ToArray());

            var rootFolder = list.RootFolder;
            rootFolder.Properties["IsPublishingCatalog"] = "True";
            rootFolder.Properties["PublishingCatalogSettings"] = "{\"FurlFields\":[" + friendlyUrlFieldsProperty + "],\"TaxonomyFieldMap\":[]}";

            rootFolder.Properties["vti_indexedpropertykeys"] = "UAB1AGIAbABpAHMAaABpAG4AZwBDAGEAdABhAGwAbwBnAFMAZQB0AHQAaQBuAGcAcwA=|SQBzAFAAdQBiAGwAaQBzAGgAaQBuAGcAQwBhAHQAYQBsAG8AZwA=|";

            rootFolder.Update();
            list.Update();

            return list;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog with a taxonomy term for navigation.
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableManagedProperties">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <returns>The SharePoint list configured as a catalog.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableManagedProperties, string taxonomyFieldMap)
        {
            return this.SetListAsCatalog(list, availableManagedProperties, taxonomyFieldMap, false);
        }

        /// <summary>
        /// Set a SharePoint as a product catalog with a taxonomy term for navigation.
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableManagedProperties">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <param name="activateAnonymousAccess">if set to <c>true</c> [activate anonymous access].</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableManagedProperties, string taxonomyFieldMap, bool activateAnonymousAccess)
        {
            this.logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

            var catalogList = this.SetListAsCatalog(list, availableManagedProperties, activateAnonymousAccess);
            var rootFolder = catalogList.RootFolder;

            // Set current culture to be able to get the "Title" of the list
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)list.ParentWeb.Language);

            // Format property
            var taxonomyField = list.Fields.GetFieldByInternalName(taxonomyFieldMap) as TaxonomyField;
            var title = taxonomyField.Title;
            var managedPropertyName = "owstaxid" + taxonomyFieldMap;
            var fieldId = taxonomyField.Id;
            var termSetId = taxonomyField.TermSetId;
            var termStoreId = taxonomyField.SspId;
            var termId = "00000000-0000-0000-0000-000000000000";

            var fields = new Collection<string>();

            fields.Add("{\"FieldDisplayName\":\"" + title + "\"");
            fields.Add("\"FieldManagedPropertyName\":\"" + managedPropertyName + "\"");
            fields.Add("\"FieldId\":\"" + fieldId + "\"");
            fields.Add("\"TermSetId\":\"" + termSetId + "\"");
            fields.Add("\"IsSelected\":true");
            fields.Add("\"TermStoreId\":\"" + termStoreId + "\"");
            fields.Add("\"TermId\":\"" + termId + "\"}");

            var taxonomyFieldMapProperty = "\"TaxonomyFieldMap\":[" + string.Join(",", fields.ToArray()) + "]";

            var newValue = rootFolder.Properties["PublishingCatalogSettings"].ToString().Replace("\"TaxonomyFieldMap\":[]", taxonomyFieldMapProperty);

            rootFolder.Properties["PublishingCatalogSettings"] = newValue;

            rootFolder.Update();
            list.Update();

            return list;
        }

        /// <summary>
        /// Ensure a catalog
        /// </summary>
        /// <param name="web">The web object</param>
        /// <param name="catalog">The catalog</param>
        /// <returns>The list object</returns>
        public SPList EnsureCatalog(SPWeb web, CatalogInfo catalog)
        {
            // Set current culture to be able to set the "Title" of the list
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);

            // Create the list if doesn't exists
            var list = this.listHelper.EnsureList(web, catalog);

            if (catalog.TaxonomyFieldMap == null)
            {
                // Set the list as catalog without navigation
                if (catalog.IsAnonymous)
                {
                    this.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), true);
                }
                else
                {
                    this.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name));
                }
            }
            else
            {
                // Set the list as catalog with navigation term
                if (catalog.IsAnonymous)
                {
                    this.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), catalog.TaxonomyFieldMap.InternalName, true);
                }
                else
                {
                    this.SetListAsCatalog(list, catalog.ManagedProperties.Select(x => x.Name), catalog.TaxonomyFieldMap.InternalName);
                }

                // Enforce unique values on the navigation column if neccessary
                if (catalog.EnforceUniqueNavigationValues)
                {
                    var field = list.Fields.GetFieldByInternalName(catalog.TaxonomyFieldMap.InternalName);
                    if (field != null)
                    {
                        // A SPField must be indexed before enforce unique values
                        field.Indexed = true;
                        field.Update();

                        field.EnforceUniqueValues = true;
                        field.Update();

                        list.Update();
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Ensure catalogs in the web
        /// </summary>
        /// <param name="web">The web</param>
        /// <param name="catalogs">The catalogs</param>
        /// <returns>The catalogs list</returns>
        public IEnumerable<SPList> EnsureCatalog(SPWeb web, ICollection<CatalogInfo> catalogs)
        {
            var catalogList = new List<SPList>();

            foreach (CatalogInfo catalog in catalogs)
            {
                catalogList.Add(this.EnsureCatalog(web, catalog));
            }

            return catalogList;
        }

        /// <summary>
        /// Method to get a CatalogConnectionSettings from the site
        /// </summary>
        /// <param name="site">The SPSite to get the connection from</param>
        /// <param name="webAbsoluteUrl">The server relative url where the catalog belong</param>
        /// <param name="catalogRootUrl">The root url of the catalog.</param>
        /// <returns>A catalogConnectionSettings object</returns>
        public CatalogConnectionSettings GetCatalogConnectionSettings(SPSite site, string webAbsoluteUrl, string catalogRootUrl)
        {
            string listToken = "lists";
            string catalogPath = string.Empty;
            var tokens = catalogRootUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (tokens.Any() && tokens.First() != listToken)
            {
                tokens.Insert(0, listToken);
            }

            CatalogConnectionSettings catalogConnectionSettings = null;

            try
            {
                catalogConnectionSettings = PublishingCatalogUtility.GetPublishingCatalog(site, SPUtility.ConcatUrls(webAbsoluteUrl, string.Join("/", tokens)));
            }
            catch (InternalQueryErrorException exception)
            {
                this.logger.Error("Publishing Catalog with tokens {0} was not found on site {1}", string.Join(", ", tokens.ToArray()), site.Url);
                this.logger.Exception(exception);
            }

            return catalogConnectionSettings;
        }

        /// <summary>
        /// Delete a catalog connection
        /// </summary>
        /// <param name="site">The target site</param>
        /// <param name="catalogConnectionInfo">The catalog connection information</param>
        public void DeleteCatalogConnection(SPSite site, CatalogConnectionInfo catalogConnectionInfo)
        {
            // Get the catalog
            var catalog = catalogConnectionInfo.Catalog;

            // Be careful, you must launch a search crawl before creating a catalog connection.
            // If a previous connection with the same catalog root folder ULR is already exists, this one will be taken instead of your new catalog
            var connectionSettings = this.GetCatalogConnectionSettings(site, catalogConnectionInfo.SourceWeb.Url, catalog.RootFolderUrl);

            if (connectionSettings != null)
            {
                this.DeleteCatalogConnection(site, connectionSettings);
            }
        }

        /// <summary>
        /// Creates a new catalog connection
        /// </summary>
        /// <param name="site">The target site</param>
        /// <param name="catalogConnectionInfo">The catalog connection information</param>
        /// <param name="overwrite">True if the connection must be override. False otherwise</param>
        public void EnsureCatalogConnection(SPSite site, CatalogConnectionInfo catalogConnectionInfo, bool overwrite)
        {
            // Get the catalog
            var catalog = catalogConnectionInfo.Catalog;

            // Be careful, you must launch a search crawl before creating a catalog connection.
            // If a previous connection with the same catalog root folder ULR is already exists, this one will be taken instead of your new catalog
            var connectionSettings = this.GetCatalogConnectionSettings(site, catalogConnectionInfo.SourceWeb.Url, catalog.RootFolderUrl);

            if (connectionSettings != null)
            {
                // Configure additional catalog connection settings
                connectionSettings.CatalogItemUrlRewriteTemplate = catalogConnectionInfo.CatalogItemUrlRewriteTemplate;
                connectionSettings.CatalogTaxonomyManagedProperty = catalogConnectionInfo.CatalogTaxonomyManagedProperty;
                connectionSettings.RewriteCatalogItemUrls = catalogConnectionInfo.RewriteCatalogItemUrls;

                // Rename the catalog (otherwise, can cause "The value must be at most 64 characters long" error 
                // because of the name of the connection is generated automatically by SharePoint
                connectionSettings.CatalogName = catalogConnectionInfo.SourceWeb.Title + " - " + catalogConnectionInfo.SourceWeb.Locale.Name + " - " +
                                                 catalogConnectionInfo.Catalog.DisplayName;

                connectionSettings.IsManualCatalogItemUrlRewriteTemplate =
                    catalogConnectionInfo.IsManualCatalogItemUrlRewriteTemplate;
                connectionSettings.IsReusedWithPinning = catalogConnectionInfo.IsReusedWithPinning;
                connectionSettings.CatalogItemUrlRewriteTemplate = catalogConnectionInfo.CatalogItemUrlRewriteTemplate;

                // Update the publishing web infos
                connectionSettings.ConnectedWebId = catalogConnectionInfo.TargetWeb.ID;
                connectionSettings.ConnectedWebServerRelativeUrl = catalogConnectionInfo.TargetWeb.ServerRelativeUrl;

                // Create the connection
                this.CreateCatalogConnection(site, connectionSettings, overwrite);
            }
            else
            {
                this.logger.Info(
                    "Connection information not found for the catalog {0}. Maybe you forgot to start a search crawl before?", catalog.DisplayName);
            }
        }

        /// <summary>
        /// Method to create a catalog connection
        /// </summary>
        /// <param name="site">The site where to create the connection</param>
        /// <param name="catalogConnectionSettings">The catalog connection settings to create</param>
        /// <param name="overwriteIfExist">if true and existing, the connection will be deleted then recreated</param>
        public void CreateCatalogConnection(SPSite site, CatalogConnectionSettings catalogConnectionSettings, bool overwriteIfExist)
        {
            var catalogManager = new CatalogConnectionManager(site);

            // If catalog connection exist
            if (catalogManager.Contains(catalogConnectionSettings.CatalogUrl))
            {
                if (overwriteIfExist)
                {
                    // Delete the existing connection
                    this.logger.Info("Deleting catalog connection: " + catalogConnectionSettings.CatalogUrl);
                    catalogManager.DeleteCatalogConnection(catalogConnectionSettings.CatalogUrl);
                    catalogManager.Update();

                    // Add connection to the catalog manager
                    this.logger.Info("Creating catalog connection: " + catalogConnectionSettings.CatalogUrl);
                    catalogManager.AddCatalogConnection(catalogConnectionSettings);
                    catalogManager.Update();
                }
            }
            else
            {
                this.logger.Info("Creating catalog connection: " + catalogConnectionSettings.CatalogUrl);
                catalogManager.AddCatalogConnection(catalogConnectionSettings);
                catalogManager.Update();
            }
        }

        /// <summary>
        /// Delete a catalog connection
        /// </summary>
        /// <param name="site">The site where to delete the connection</param>
        /// <param name="catalogConnectionSettings">The catalog connection settings to create</param>
        public void DeleteCatalogConnection(SPSite site, CatalogConnectionSettings catalogConnectionSettings)
        {
            var catalogManager = new CatalogConnectionManager(site);

            // If catalog connection exist
            if (catalogManager.Contains(catalogConnectionSettings.CatalogUrl))
            {
                // Delete the existing connection
                this.logger.Info("Deleting catalog connection: " + catalogConnectionSettings.CatalogUrl);
                catalogManager.DeleteCatalogConnection(catalogConnectionSettings.CatalogUrl);
                catalogManager.Update();
            }
        }
    }
}
