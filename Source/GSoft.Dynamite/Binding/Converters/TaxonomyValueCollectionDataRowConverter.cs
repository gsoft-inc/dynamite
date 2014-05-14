using System;
using System.Collections.Generic;
using System.Linq;
using GSoft.Dynamite.Taxonomy;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Binding.Converters
{
    /// <summary>
    /// The converter for multi-value taxonomy fields.
    /// </summary>
    public class TaxonomyValueCollectionDataRowConverter : DataRowValueConverter
    {
        private ITaxonomyService taxonomyService;

        /// <summary>
        /// Converter constructor with dependency injection
        /// </summary>
        /// <param name="taxonomyService">Taxonomy service utility</param>
        public TaxonomyValueCollectionDataRowConverter(ITaxonomyService taxonomyService)
        {
            this.taxonomyService = taxonomyService;
        }

        #region IConverter Members

        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object Convert(object value, DataRowConversionArguments arguments)
        {
            TaxonomyValueCollection convertedValues = null;

            if (value == DBNull.Value)
            {
                return null;
            }

            var taxonomyFieldValueCollection = value as TaxonomyFieldValueCollection;
            if (taxonomyFieldValueCollection == null)
            {
                var stringValue = value as string;
                if (!string.IsNullOrEmpty(stringValue))
                {

                    var fieldObject = arguments.FieldCollection.Cast<SPField>()
                        .FirstOrDefault(f => f.InternalName == arguments.ValueKey);

                    if (fieldObject != null)
                    {
                        taxonomyFieldValueCollection = new TaxonomyFieldValueCollection(fieldObject);
                        taxonomyFieldValueCollection.PopulateFromLabelGuidPairs(stringValue);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            if (taxonomyFieldValueCollection != null)
            {
                if (SPContext.Current != null)
                {
                    // Resolve the Term from the term store, because we want all Labels and we want to
                    // create the TaxonomyValue object with a label in the correct LCID (we want one with
                    // LCID = CurrentUICUlture.LCID
                    var underLyingTerms = new List<Term>();
                    foreach (TaxonomyFieldValue taxonomyFieldValue in taxonomyFieldValueCollection)
                    {
                        if (!string.IsNullOrEmpty(taxonomyFieldValue.TermGuid))
                        {
                            var foundTerm = this.taxonomyService.GetTermForId(SPContext.Current.Site, new Guid(taxonomyFieldValue.TermGuid));

                            if (foundTerm != null)
                            {
                                underLyingTerms.Add(foundTerm);
                            }
                        }
                    }

                    convertedValues = new TaxonomyValueCollection(underLyingTerms);
                }
                else
                {
                    // We don't have access to a SPContext (needed to use the TaxonomyService), so we need to 
                    // fall back on the non-UICulture-respecting TaxonomyValueCollection constructor
                    convertedValues = new TaxonomyValueCollection(taxonomyFieldValueCollection);
                }
            }

            return convertedValues;
        }

        /// <summary>
        /// Converts the specified value back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public override object ConvertBack(object value, DataRowConversionArguments arguments)
        {
            var terms = value as TaxonomyValueCollection;
            TaxonomyFieldValueCollection newTaxonomyFieldValueCollection = null;

            var taxonomyField = (TaxonomyField)arguments.FieldCollection.GetField(arguments.ValueKey);
            newTaxonomyFieldValueCollection = new TaxonomyFieldValueCollection(taxonomyField);

            var noteField = arguments.FieldCollection[taxonomyField.TextField];

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
