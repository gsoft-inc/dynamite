using System.Linq;
using GSoft.Dynamite.Sharepoint.ValueTypes;

namespace GSoft.Dynamite.Sharepoint.Binding.Converters
{
    /// <summary>
    /// The converter for multi-value taxonomy fields.
    /// </summary>
    public class TaxonomyValueCollectionConverter : SharePointListItemValueConverter
    {
        #region IConverter Members

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object Convert(object value, SharePointListItemConversionArguments arguments)
        {
            var taxonomyValues = value as TaxonomyFieldValueCollection;
            if (taxonomyValues == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    taxonomyValues = new TaxonomyFieldValueCollection(stringValue);
                }
            }

            return taxonomyValues != null ? new TaxonomyValueCollection(taxonomyValues) : null;
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, SharePointListItemConversionArguments arguments)
        {
            var terms = value as TaxonomyValueCollection;
            TaxonomyFieldValueCollection newTaxonomyFieldValueCollection = null;

            TaxonomyField taxonomyField = (TaxonomyField)arguments.ListItem.Fields.GetField(arguments.ValueKey);
            newTaxonomyFieldValueCollection = new TaxonomyFieldValueCollection(taxonomyField);

            var noteField = arguments.ListItem.Fields[taxonomyField.TextField];

            if (terms != null && terms.Count > 0)
            {
                string labelGuidPairs = string.Join(";", terms.Select(term => term.Label + "|" + term.Id).ToArray());

                // PopulateFromLabelGuidPairs takes care of looking up the WssId values and creating new items in the TaxonomyHiddenList if needed.
                // Main taxonomy field value format: WssID;#Label;WssID;#Label;WssID;#Label...
                // TODO - Make sure we support sub-level terms with format: WssID;#Label|RootTermGuid|...|ParentTermGuid|TermGuid
                // Reference: http://msdn.microsoft.com/en-us/library/ee577520.aspx
                newTaxonomyFieldValueCollection.PopulateFromLabelGuidPairs(labelGuidPairs);

                // Must write associated note field as well as the main taxonomy field.
                // Note field value format: Label|Guid;Label|Guid;Label|Guid...
                // Reference: http://nickhobbs.wordpress.com/2012/02/21/sharepoint-2010-how-to-set-taxonomy-field-values-programmatically/
                arguments.FieldValues[noteField.InternalName] = labelGuidPairs;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                arguments.FieldValues[noteField.InternalName] = null;
            }

            return newTaxonomyFieldValueCollection;
        }

        #endregion
    }
}
