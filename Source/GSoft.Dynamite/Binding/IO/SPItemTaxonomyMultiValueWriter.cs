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
    /// Writes Taxonomy Multi values to SharePoint list items.
    /// </summary>
    public class SPItemTaxonomyMultiValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a Taxonomy Multi field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var termInfos = fieldValueInfo.Value as TaxonomyFullValueCollection;
            TaxonomyFieldValueCollection newTaxonomyFieldValueCollection = null;

            TaxonomyField taxonomyField = (TaxonomyField)item.Fields.GetField(fieldValueInfo.FieldInfo.InternalName);
            newTaxonomyFieldValueCollection = new TaxonomyFieldValueCollection(taxonomyField);

            var noteField = item.Fields[taxonomyField.TextField];

            if (termInfos != null && termInfos.Count > 0)
            {
                string labelGuidPairs = string.Join(";", termInfos.Where(termInfo => termInfo.Term != null).Select(termInfo => termInfo.Term.Label + "|" + termInfo.Term.Id).ToArray());

                // PopulateFromLabelGuidPairs takes care of looking up the WssId values and creating new items in the TaxonomyHiddenList if needed.
                // Main taxonomy field value format: WssID;#Label;WssID;#Label;WssID;#Label...
                // TODO - Make sure we support sub-level terms with format: WssID;#Label|RootTermGuid|...|ParentTermGuid|TermGuid
                // Reference: http://msdn.microsoft.com/en-us/library/ee577520.aspx
                newTaxonomyFieldValueCollection.PopulateFromLabelGuidPairs(labelGuidPairs);

                // Must write associated note field as well as the main taxonomy field.
                // Note field value format: Label|Guid;Label|Guid;Label|Guid...
                // Reference: http://nickhobbs.wordpress.com/2012/02/21/sharepoint-2010-how-to-set-taxonomy-field-values-programmatically/
                item[noteField.InternalName] = labelGuidPairs;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                item[noteField.InternalName] = null;
            }

            item[fieldValueInfo.FieldInfo.InternalName] = newTaxonomyFieldValueCollection;

            return item;
        }
    }
}