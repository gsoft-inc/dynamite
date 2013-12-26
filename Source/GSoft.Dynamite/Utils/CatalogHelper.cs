using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint;
using System.Globalization;
using System.Threading;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Utils
{
    /// <summary>
    /// Helper class for Cross Site Publishing operations
    /// </summary>
    public class CatalogHelper
    {
        /// <summary>
        /// Dynamite Helpers
        /// </summary>
        private ListHelper _listHelper;

        public CatalogHelper(ListHelper listHelper)
        {
            _listHelper = listHelper;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog without navigation term associated
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <returns>The SharePoint list.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields)
        {
            // Add properties for catalog publishing on the root folder
            list.IndexedRootFolderPropertyKeys.Add("PublishingCatalogSettings");
            list.IndexedRootFolderPropertyKeys.Add("IsPublishingCatalog");

            // Break list inheritance for anonymous access
            list.BreakRoleInheritance(true, false);

            // Allow anonymous acces on the list
            list.AnonymousPermMask64 = SPBasePermissions.AnonymousSearchAccessList;

            var fieldList = new Collection<string>();

            // For fields name, you need to pass the internal name of the column directly followed by "OWSTEXT"
            foreach (var availableField in availableFields)
            {
                fieldList.Add("\"" + availableField + "\"");
            }

            var fUrlFieldsProperty = String.Join(",", fieldList.ToArray());

            var rootFolder = list.RootFolder;
            rootFolder.Properties["IsPublishingCatalog"] = "True";
            rootFolder.Properties["PublishingCatalogSettings"] = "{\"FurlFields\":[" + fUrlFieldsProperty + "],\"TaxonomyFieldMap\":[]}";

            rootFolder.Properties["vti_indexedpropertykeys"] = "UAB1AGIAbABpAHMAaABpAG4AZwBDAGEAdABhAGwAbwBnAFMAZQB0AHQAaQBuAGcAcwA=|SQBzAFAAdQBiAGwAaQBzAGgAaQBuAGcAQwBhAHQAYQBsAG8AZwA=|";

            rootFolder.Update();
            list.Update();

            return list;
        }

        /// <summary>
        /// Set a SharePoint as a product catalog with a taxonomy term for navigation
        /// </summary>
        /// <param name="list">The SharePoint list.</param>
        /// <param name="availableFields">List of internal field names that are available through the catalog.</param>
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation.</param>
        /// <returns>The SharePoint list.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields,
            string taxonomyFieldMap)
        {
            var spList = this.SetListAsCatalog(list, availableFields);
            var rootFolder = spList.RootFolder;

            // Set current culture to be able to get the "Title" of the list
            CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture =
                new CultureInfo((int)list.ParentWeb.Language);

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

            var taxonomyFieldMapProperty = "\"TaxonomyFieldMap\":[" + String.Join(",", fields.ToArray()) + "]";

            var oldValue = rootFolder.Properties["PublishingCatalogSettings"];
            var newValue = rootFolder.Properties["PublishingCatalogSettings"].ToString().Replace("\"TaxonomyFieldMap\":[]", taxonomyFieldMapProperty);

            rootFolder.Properties["PublishingCatalogSettings"] = newValue;

            rootFolder.Update();
            list.Update();

            return list;
        }
    }
}
