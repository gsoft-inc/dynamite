using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class for Cross Site Publishing operations
    /// </summary>
    public class CatalogHelper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="logger">The logger</param>
        public CatalogHelper(ILogger logger)
        {
            this._logger = logger;
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
    }
}
