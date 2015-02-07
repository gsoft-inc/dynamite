using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Taxonomy Multi values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class TaxonomyFullValueCollectionWriter : BaseValueWriter<TaxonomyFullValueCollection>
    {
        /// <summary>
        /// Writes a Taxonomy Multi field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var termInfos = fieldValueInfo.Value as TaxonomyFullValueCollection;
            TaxonomyFieldValueCollection newTaxonomyFieldValueCollection = null;

            TaxonomyField taxonomyField = (TaxonomyField)item.Fields.GetField(fieldValueInfo.FieldInfo.InternalName);

            var noteField = item.Fields[taxonomyField.TextField];

            if (termInfos != null && termInfos.Count > 0)
            {
                List<string> labelGuidPairsListOutParam = new List<string>();
                newTaxonomyFieldValueCollection = this.CreateSharePointTaxonomyFieldValue(taxonomyField, termInfos, labelGuidPairsListOutParam);

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
            var withDefaultVal = (FieldInfo<TaxonomyFullValueCollection>)fieldValueInfo.FieldInfo;

            if (withDefaultVal.DefaultValue != null)
            {               
                var taxonomyFieldValueCollection = this.CreateSharePointTaxonomyFieldValue(sharepointTaxonomyField, withDefaultVal.DefaultValue, null);
                string collectionValidatedString = sharepointTaxonomyField.GetValidatedString(taxonomyFieldValueCollection);

                sharepointTaxonomyField.DefaultValue = collectionValidatedString;
                sharepointTaxonomyField.Update();
            }
            else
            {
                sharepointTaxonomyField.DefaultValue = null;
            }
        }

        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }

        private TaxonomyFieldValueCollection CreateSharePointTaxonomyFieldValue(
            TaxonomyField sharepointTaxonomyField, 
            TaxonomyFullValueCollection dynamiteCollection, 
            List<string> labelGuidPairsListOutParam)
        {
            if (labelGuidPairsListOutParam == null)
            {
                labelGuidPairsListOutParam = new List<string>();
            }

            TaxonomyFieldValueCollection sharePointValueCollection = null;

            foreach (var taxonomyFullValue in dynamiteCollection)
            {
                string labelGuidPair = TaxonomyItem.NormalizeName(taxonomyFullValue.Term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                                + taxonomyFullValue.Term.Id.ToString().ToUpperInvariant(); ;

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