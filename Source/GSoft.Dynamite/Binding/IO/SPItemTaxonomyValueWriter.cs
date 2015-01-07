using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Writes Taxonomy values to SharePoint list items.
    /// </summary>
    public class SPItemTaxonomyValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a taxonomy field value to a SPListItem.
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var term = fieldValueInfo.Value as TaxonomyValue;
            TaxonomyFieldValue newTaxonomyFieldValue = null;

            TaxonomyField taxonomyField = (TaxonomyField)item.Fields.GetField(fieldValueInfo.FieldInfo.InternalName);
            newTaxonomyFieldValue = new TaxonomyFieldValue(taxonomyField);

            var noteField = item.Fields[taxonomyField.TextField];

            if (term != null)
            {
                string labelGuidPair = term.Label + "|" + term.Id;

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

            return item;
        }
    }
}