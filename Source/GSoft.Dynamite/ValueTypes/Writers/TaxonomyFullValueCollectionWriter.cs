using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Taxonomy Multi values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class TaxonomyValueCollectionWriter : BaseValueWriter<TaxonomyValueCollection>
    {
        /// <summary>
        /// Writes a Taxonomy Multi field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var termInfos = fieldValueInfo.Value as TaxonomyValueCollection;
            TaxonomyFieldValueCollection newTaxonomyFieldValueCollection = null;

            TaxonomyField taxonomyField = (TaxonomyField)item.Fields.GetField(fieldValueInfo.FieldInfo.InternalName);

            var noteField = item.Fields[taxonomyField.TextField];

            if (termInfos != null && termInfos.Count > 0)
            {
                List<string> labelGuidPairsListOutParam = new List<string>();
                newTaxonomyFieldValueCollection = CreateSharePointTaxonomyFieldValue(taxonomyField, termInfos, labelGuidPairsListOutParam);

                item[taxonomyField.Id] = newTaxonomyFieldValueCollection;
              
                // Must write associated note field as well as the main taxonomy field.
                // Note field value format: Label|Guid;Label|Guid;Label|Guid...
                // Reference: http://nickhobbs.wordpress.com/2012/02/21/sharepoint-2010-how-to-set-taxonomy-field-values-programmatically/
                string labelGuidPairsAsString = string.Join(";", labelGuidPairsListOutParam.ToArray());
                item[noteField.InternalName] = labelGuidPairsAsString;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                item[noteField.InternalName] = null;
            }

            item[fieldValueInfo.FieldInfo.InternalName] = newTaxonomyFieldValueCollection;
        }

        /// <summary>
        /// Writes a taxonomy multi value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var sharepointTaxonomyField = (TaxonomyField)parentFieldCollection[fieldValueInfo.FieldInfo.Id];
            var defaultVal = (TaxonomyValueCollection)fieldValueInfo.Value;

            if (defaultVal != null)
            {               
                var taxonomyFieldValueCollection = CreateSharePointTaxonomyFieldValue(sharepointTaxonomyField, defaultVal, null);
                string collectionValidatedString = sharepointTaxonomyField.GetValidatedString(taxonomyFieldValueCollection);

                sharepointTaxonomyField.DefaultValue = collectionValidatedString;
            }
            else
            {
                sharepointTaxonomyField.DefaultValue = null;
            }

            sharepointTaxonomyField.Update();
        }

        /// <summary>
        /// Writes a field value as an SPFolder's default column value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update a field's default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (TaxonomyValueCollection)fieldValueInfo.Value;
            var list = folder.ParentWeb.Lists[folder.ParentListId];
            var taxonomyField = (TaxonomyField)list.Fields[fieldValueInfo.FieldInfo.Id];
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(list);

            if (defaultValue != null)
            {
                var taxonomyFieldValueCollection = CreateSharePointTaxonomyFieldValue(taxonomyField, defaultValue, null);
                string collectionValidatedString = taxonomyField.GetValidatedString(taxonomyFieldValueCollection);

                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, collectionValidatedString);
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();
        }

        private static TaxonomyFieldValueCollection CreateSharePointTaxonomyFieldValue(
            TaxonomyField sharepointTaxonomyField, 
            TaxonomyValueCollection dynamiteCollection, 
            List<string> labelGuidPairsListOutParam)
        {
            if (labelGuidPairsListOutParam == null)
            {
                labelGuidPairsListOutParam = new List<string>();
            }

            TaxonomyFieldValueCollection sharePointValueCollection = null;

            foreach (var TaxonomyValue in dynamiteCollection)
            {
                string labelGuidPair = TaxonomyItem.NormalizeName(TaxonomyValue.Term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                                + TaxonomyValue.Term.Id.ToString().ToUpperInvariant();

                labelGuidPairsListOutParam.Add(labelGuidPair);
            }

            if (labelGuidPairsListOutParam.Count >= 1)
            {
                sharePointValueCollection = new TaxonomyFieldValueCollection(sharepointTaxonomyField);

                labelGuidPairsListOutParam.ForEach(labelGuidPair =>
                {
                    TaxonomyFieldValue taxoFieldValue = new TaxonomyFieldValue(sharepointTaxonomyField);
                    taxoFieldValue.PopulateFromLabelGuidPair(labelGuidPair);

                    sharePointValueCollection.Add(taxoFieldValue);
                });
            }

            return sharePointValueCollection;
        }
    }
}