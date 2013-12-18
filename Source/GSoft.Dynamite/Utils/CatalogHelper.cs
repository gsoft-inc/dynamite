using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SharePoint;

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
                fieldList.Add("\"" + availableField + "OWSTEXT\"");
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
        /// <param name="taxonomyFieldMap">The taxonomy field that will be used for navigation. NOT IMPLEMENTED YET.</param>
        /// <returns>The SharePoint list.</returns>
        public SPList SetListAsCatalog(SPList list, IEnumerable<string> availableFields,
            string taxonomyFieldMap)
        {
            // TODO: Format the taxonomyFieldMap as follow
            // {"FurlFields":["MyColumn2OWSTEXT","MyColumn1OWSTEXT","owstaxidMyLangue"],"TaxonomyFieldMap":[{"FieldDisplayName":"MyLangue","FieldManagedPropertyName":"owstaxidMyLangue","FieldId":"3a35406b-97f3-4711-9a2c-bb777c1e95ab","TermSetId":"e9a57134-ff11-4025-af3b-3d7bd1972722","IsSelected":true,"TermStoreId":"baf66b76-0edf-4f3d-ad84-539b8d181b34","TermId":"00000000-0000-0000-0000-000000000000"}]}
            
            throw new NotImplementedException();
        }
    }
}
