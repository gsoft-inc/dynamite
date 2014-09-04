using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Definitions;
using GSoft.Dynamite.Helpers;
using GSoft.Dynamite.Lists;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing;
using Microsoft.SharePoint.Taxonomy;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class for Cross Site Publishing operations
    /// </summary>
    public class CatalogHelper
    {
        private readonly ILogger _logger;
        private readonly ListHelper _listHelper;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        public CatalogHelper(ILogger logger, ListHelper listHelper)
        {
            this._logger = logger;
            this._listHelper = listHelper;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields)
        {
            return this.SetListAsCatalog(list, availableFields, false);
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// Note: For more information, see PublishingCatalogUtility in Microsoft.SharePoint.Publishing
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <param name="activateAnonymousAccess">if set to <c>true</c> [activate anonymous access].</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields, bool activateAnonymousAccess)
        {
            this._logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

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
            foreach (var availableField in availableFields)
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
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <returns>The SharePoint list configured as a catalog.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields, string taxonomyFieldMap)
        {
            return this.SetListAsCatalog(list, availableFields, taxonomyFieldMap, false);
        }

        /// <summary>
        /// Set a SharePoint as a product catalog with a taxonomy term for navigation.
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <param name="activateAnonymousAccess">if set to <c>true</c> [activate anonymous access].</param>
        /// <returns>
        /// The SharePoint list configured as a catalog.
        /// </returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields, string taxonomyFieldMap, bool activateAnonymousAccess)
        {
            this._logger.Info("Start method 'SetListAsCatalog' for list: '{0}'", list.RootFolder.Url);

            var catalogList = this.SetListAsCatalog(list, availableFields, activateAnonymousAccess);
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

        public SPList EnsureCatalog(SPWeb web, CatalogInfo catalog)
        {
            // Set current culture to be able to set the "Title" of the list
            Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)web.Language);

            // Create the list if doesn't exists
            var list = this._listHelper.EnsureList(web, catalog);

            // Rename List
            list.Title = catalog.DisplayName;
            list.Update();

            // Remove Item Content Type
            if (catalog.RemoveDefaultContentType)
            {
                // If content type is direct child of item, remove it
                this._listHelper.RemoveItemContentType(list);
            }

            // Add All Content Types
            this._listHelper.EnsureContentType(list, catalog.ContentTypes);

            return list;
        }

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
        /// <param name="serverRelativeUrl">The server relative url where the catalog belong</param>
        /// <param name="catalogRootUrl">The root url of the catalog.</param>
        /// <returns>A catalogConnectionSettings object</returns>
        public CatalogConnectionSettings GetCatalogConnectionSettings(SPSite site, string serverRelativeUrl, string catalogRootUrl)
        {
            string listToken = "lists";
            string catalogPath = string.Empty;
            var tokens = catalogRootUrl.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (tokens.Any() && tokens.First() != listToken)
            {
                tokens.Insert(0, listToken);
            }

            return PublishingCatalogUtility.GetPublishingCatalog(site, SPUtility.ConcatUrls(serverRelativeUrl, string.Join("/", tokens)));
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
                    this._logger.Info("Deleting catalog connection: " + catalogConnectionSettings.CatalogUrl);
                    catalogManager.DeleteCatalogConnection(catalogConnectionSettings.CatalogUrl);
                    catalogManager.Update();

                    // Add connection to the catalog manager
                    this._logger.Info("Creating catalog connection: " + catalogConnectionSettings.CatalogUrl);
                    catalogManager.AddCatalogConnection(catalogConnectionSettings);
                    catalogManager.Update();
                }
            }
            else
            {
                this._logger.Info("Creating catalog connection: " + catalogConnectionSettings.CatalogUrl);
                catalogManager.AddCatalogConnection(catalogConnectionSettings);
                catalogManager.Update();
            }
        }
    }
}
