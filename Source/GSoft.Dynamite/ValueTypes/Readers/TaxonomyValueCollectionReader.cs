using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Taxonomy;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes.Readers
{
    /// <summary>
    /// Reads TaxonomyMulti-based field values
    /// </summary>
    public class TaxonomyValueCollectionReader : BaseValueReader<TaxonomyValueCollection>
    {
        /// <summary>
        /// Reads a field value from a list item
        /// </summary>
        /// <param name="item">The list item we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The value extracted from the list item's field</returns>
        public override TaxonomyValueCollection ReadValueFromListItem(SPListItem item, string fieldInternalName)
        {
            var fieldValue = item[fieldInternalName];

            if (fieldValue != null)
            {
                var taxFieldVal = (TaxonomyFieldValueCollection)fieldValue;
                var taxValueCollection = new TaxonomyValueCollection(taxFieldVal);

                var field = (TaxonomyField)item.Fields.GetFieldByInternalName(fieldInternalName);

                InitTaxonomyContextForValues(taxValueCollection, field, item.Web.Site);

                return taxValueCollection;
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a list item version
        /// </summary>
        /// <param name="itemVersion">The list item version we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field in the item's columns</param>
        /// <returns>The ImageValue extracted from the list item's field</returns>
        public override TaxonomyValueCollection ReadValueFromListItemVersion(SPListItemVersion itemVersion, string fieldInternalName)
        {
            var fieldValue = itemVersion[fieldInternalName];

            if (fieldValue != null)
            {
                var taxFieldVal = (TaxonomyFieldValueCollection)fieldValue;
                var taxValueCollection = new TaxonomyValueCollection(taxFieldVal);

                var field = (TaxonomyField)itemVersion.Fields.GetFieldByInternalName(fieldInternalName);

                InitTaxonomyContextForValues(taxValueCollection, field, itemVersion.ListItem.Web.Site);

                return taxValueCollection;
            }

            return null;
        }

        /// <summary>
        /// Reads a field value from a DataRow returned by a CAML query
        /// </summary>
        /// <param name="web">The context's web</param>
        /// <param name="dataRowFromCamlResult">The CAML-query-result data row we want to extract a field value from</param>
        /// <param name="fieldInternalName">The key to find the field among the data row cells</param>
        /// <returns>The value extracted from the data row's corresponding cell</returns>
        public override TaxonomyValueCollection ReadValueFromCamlResultDataRow(SPWeb web, DataRow dataRowFromCamlResult, string fieldInternalName)
        {
            var fieldValue = dataRowFromCamlResult[fieldInternalName];

            if (fieldValue != null && fieldValue != System.DBNull.Value)
            {
                var site = web.Site;
                var field = (TaxonomyField)site.RootWeb.Fields.GetFieldByInternalName(fieldInternalName);

                var taxFieldVal = new TaxonomyFieldValueCollection(field);
                taxFieldVal.PopulateFromLabelGuidPairs(fieldValue.ToString());

                var taxValueCollection = new TaxonomyValueCollection(taxFieldVal);

                // Watch out! Here, we're going to use the Site Collection's site column to determine
                // the taxonomy context. This means that if the item comes from a list where the 
                // TermStoreMapping on the list column is different than on the site column, we're
                // going to initialize the wrong context for the item here.
                InitTaxonomyContextForValues(taxValueCollection, field, site);

                return taxValueCollection;
            }

            return null;
        }

        private static void InitTaxonomyContextForValues(TaxonomyValueCollection taxValueCollection, TaxonomyField field, SPSite site)
        {
            if (field.SspId != null && field.SspId != Guid.Empty)
            {
                var taxonomySession = new TaxonomySession(site);
                var termStore = taxonomySession.TermStores[field.SspId];
                var termSet = termStore.GetTermSet(field.TermSetId);

                foreach (var taxValue in taxValueCollection)
                {
                    if (field.AnchorId != null && field.AnchorId != Guid.Empty)
                    {
                        // Taxonomy picker is limited to a sub-term of the term set
                        taxValue.Context = new TaxonomyContext(new TermInfo(termSet.GetTerm(field.AnchorId)));
                    }
                    else
                    {
                        // Taxonomy picker allows you to select any term within the term
                        // set bound to the column
                        taxValue.Context = new TaxonomyContext(new TermSetInfo(termSet));
                    }
                }
            }
        }
    }
}