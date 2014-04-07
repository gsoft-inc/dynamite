using GSoft.Dynamite.Setup;

using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Helps in filling taxonomy fields
    /// </summary>
    public class TaxonomyInfo : FieldValueInfo, ITaxonomyInfo
    {
        /// <summary>
        /// Sets the value of the default taxonomy field in the list item to the properties of the Term object in the default language of the TermStore object.
        /// </summary>
        public Term Term { get; set; }

        /// <summary>
        /// Applies the on item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void ApplyOnItem(SPListItem item)
        {
            SPFieldCollection fields = item.ParentList.Fields;
            var fieldToSet = fields.GetFieldByInternalName(this.FieldName) as TaxonomyField;

            if (fieldToSet != null)
            {
                fieldToSet.SetFieldValue(item, this.Term);
            }
        }

        /// <summary>
        /// Sets the value of the taxonomy field in the given list item to the properties of the Term object in the default language of the TermStore object.
        /// </summary>
        /// <param name="item">The <see cref="SPListItem"/> object whose field is to be updated.</param>
        /// <param name="list">List containing the TaxonomyField to set</param>
        public void ApplyOnItem(SPListItem item, SPList list)
        {
            var fieldToSet = list.Fields.GetFieldByInternalName(this.FieldName) as TaxonomyField;

            if (fieldToSet != null)
            {
                fieldToSet.SetFieldValue(item, this.Term);
            }
        }

        /// <summary>
        /// Sets a default for a field at a location.
        /// </summary>
        /// <param name="metadata">Provides the method to set the default value for the field</param>
        /// <param name="folder"><see cref="SPFolder"/> location at which to set the default value</param>
        /// <param name="list">List of the TaxonomyField containing the validatedString corresponding to the default value.</param>
        public void ApplyFieldOnMetadata(MetadataDefaults metadata, SPFolder folder, SPList list)
        {
            var term = this.Term;
            var labelGuidPair = term.GetDefaultLabel((int)list.ParentWeb.Language) + "|" + term.Id;
            var taxonomyField = list.Fields.GetField(this.FieldName) as TaxonomyField;
            var newTaxonomyFieldValue = new TaxonomyFieldValue(taxonomyField);

            // PopulateFromLabelGuidPair takes care of looking up the WssId value and creating a new item in the TaxonomyHiddenList if needed.
            // Reference: http://msdn.microsoft.com/en-us/library/ee567833.aspx
            newTaxonomyFieldValue.PopulateFromLabelGuidPair(labelGuidPair);

            metadata.SetFieldDefault(folder, this.FieldName, newTaxonomyFieldValue.ValidatedString);
        }
    }
}