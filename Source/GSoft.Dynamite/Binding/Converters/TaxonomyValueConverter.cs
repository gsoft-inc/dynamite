using GSoft.Dynamite.Sharepoint.ValueTypes;

namespace GSoft.Dynamite.Sharepoint.Binding.Converters
{
    /// <summary>
    /// The converter for taxonomy fields.
    /// </summary>
    public class TaxonomyValueConverter : SharePointListItemValueConverter
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
            var taxonomyValue = value as TaxonomyFieldValue;

            if (taxonomyValue == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {
                    taxonomyValue = new TaxonomyFieldValue(stringValue);
                }
            }

            return taxonomyValue != null && !string.IsNullOrEmpty(taxonomyValue.TermGuid) ? new TaxonomyValue(taxonomyValue) : null;
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
            var term = value as TaxonomyValue;
            TaxonomyFieldValue newTaxonomyFieldValue = null;

            TaxonomyField taxonomyField = (TaxonomyField)arguments.ListItem.Fields.GetField(arguments.ValueKey);
            newTaxonomyFieldValue = new TaxonomyFieldValue(taxonomyField);

            var noteField = arguments.ListItem.Fields[taxonomyField.TextField];

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
                arguments.FieldValues[noteField.InternalName] = labelGuidPair;
            }
            else
            {
                // No taxonomy value, make sure to empty the note field as well
                arguments.FieldValues[noteField.InternalName] = null;
            }

            return newTaxonomyFieldValue;
        }

        #endregion
    }
}
