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
    /// Writes Taxonomy values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class TaxonomyValueWriter : BaseValueWriter<TaxonomyValue>
    {
        /// <summary>
        /// Writes a taxonomy field value to a SPListItem.
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var termInfo = fieldValueInfo.Value as TaxonomyValue;
            TaxonomyFieldValue newTaxonomyFieldValue = null;

            TaxonomyField taxonomyField = (TaxonomyField)item.Fields.GetField(fieldValueInfo.FieldInfo.InternalName);
            newTaxonomyFieldValue = new TaxonomyFieldValue(taxonomyField);

            var noteField = item.Fields[taxonomyField.TextField];

            if (termInfo != null && termInfo.Term != null)
            {
                string labelGuidPair = TaxonomyItem.NormalizeName(termInfo.Term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter + termInfo.Term.Id.ToString().ToUpperInvariant();

                // PopulateFromLabelGuidPair takes care of looking up the WssId value and creating a new item in the TaxonomyHiddenList if needed.
                // Main taxonomy field value format: WssID;#Label
                // TODO - Make sure we support sub-level terms with format: WssID;#Label|RootTermGuid|...|ParentTermGuid|TermGuid
                // Reference: http://msdn.microsoft.com/en-us/library/ee567833.aspx
                newTaxonomyFieldValue.PopulateFromLabelGuidPair(labelGuidPair);

                // Must write associated note field as well as the main taxonomy field.
                // Note field value format: Label|Guid
                // Reference: http://nickhobbs.wordpress.com/2012/02/21/sharepoint-2010-how-to-set-taxonomy-field-values-programmatically/
                item[noteField.InternalName] = labelGuidPair;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                item[noteField.InternalName] = null;
            }

            item[fieldValueInfo.FieldInfo.InternalName] = newTaxonomyFieldValue;
        }

        /// <summary>
        /// Writes a Taxonomy single value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (TaxonomyValue)fieldValueInfo.Value;
            var taxonomyField = (TaxonomyField)parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue  != null)
            {
                taxonomyField.DefaultValue = FormatTaxonomyString(taxonomyField, defaultValue);
            }
            else
            {
                taxonomyField.DefaultValue = null;
            }

            taxonomyField.Update();
        }

        /// <summary>
        /// Writes a field value as an SPFolder's default column value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update a field's default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (TaxonomyValue)fieldValueInfo.Value;
            var list = folder.ParentWeb.Lists[folder.ParentListId];
            var taxonomyField = (TaxonomyField)list.Fields[fieldValueInfo.FieldInfo.Id];
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(list);

            if (defaultValue != null)
            {
                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, FormatTaxonomyString(taxonomyField, defaultValue));
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();
        }

        private static string FormatTaxonomyString(TaxonomyField sharePointField, TaxonomyValue valueToApply)
        {
            var sharePointTaxonomyFieldValue = new TaxonomyFieldValue(sharePointField);
            string path = TaxonomyItem.NormalizeName(valueToApply.Term.Label) + TaxonomyField.TaxonomyGuidLabelDelimiter
                            + valueToApply.Term.Id.ToString().ToUpperInvariant();
            sharePointTaxonomyFieldValue.PopulateFromLabelGuidPair(path);

            return sharePointTaxonomyFieldValue.ValidatedString;
        }
    }
}