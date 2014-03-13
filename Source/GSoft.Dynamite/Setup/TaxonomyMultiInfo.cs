using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using GSoft.Dynamite;
using GSoft.Dynamite.Setup;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Helps in filling taxonomy-multi fields
    /// </summary>
    public class TaxonomyMultiInfo : FieldValueInfo, ITaxonomyMultiInfo
    {
        /// <summary>
        /// Sets the value of the default taxonomy field in the list item to the properties of the Term object in the default language of the TermStore object.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Okay")]
        public Collection<Term> Terms { get; set; }

        /// <summary>
        /// Applies the on item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void ApplyOnItem(SPListItem item)
        {
            this.ApplyOnItem(item, item.ParentList);
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
                var newTaxonomyFieldValueCollection = this.GetTaxonomyFieldValueCollection(fieldToSet);
                fieldToSet.SetFieldValue(item, newTaxonomyFieldValueCollection);
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
            var taxonomyField = list.Fields.GetField(this.FieldName) as TaxonomyField;

            if (taxonomyField != null)
            {
                var newTaxonomyFieldValueCollection = this.GetTaxonomyFieldValueCollection(taxonomyField);

                List<string> wssIdAndLabelStrings = new List<string>();

                foreach (TaxonomyFieldValue fieldValue in newTaxonomyFieldValueCollection)
                {
                    var wssId = fieldValue.WssId;
                    var label = fieldValue.Label;

                    wssIdAndLabelStrings.Add(wssId + ";#" + label);
                }

                // Tax field value collection looks like this: "97;#Human resources;#96;#IT Services Portal" where "WSSidTerm1;#Term1Label;#WssidTerm2;#Term2Label"
                string taxFieldValueCollectionAsString = string.Join(";#", wssIdAndLabelStrings.ToArray());

                metadata.SetFieldDefault(folder, this.FieldName, taxFieldValueCollectionAsString);
            }
        }

        private static SPContentType FindContentTypeWithField(SPContentTypeCollection contentTypeCollection, TaxonomyField fieldToSet)
        {
            return contentTypeCollection.Cast<SPContentType>().FirstOrDefault(ct =>
            {
                return ct.Fields.Cast<SPField>().Any(spField => spField.InternalName == fieldToSet.InternalName);
            });
        }

        private TaxonomyFieldValueCollection GetTaxonomyFieldValueCollection(TaxonomyField fieldToSet)
        {
            byte someData = 0;

            // Create a temporary item to create the proper TaxonomyHiddenList items necessary to initialize the WSSids for the folder metadata defaults
            string tempPageName = string.Format(CultureInfo.InvariantCulture, "Temp-{0}.aspx", Guid.NewGuid().ToString());
            SPFile tempFile = ((SPDocumentLibrary)fieldToSet.ParentList).RootFolder.Files.Add(tempPageName, new byte[1] { someData });
            SPListItem tempItem = tempFile.Item;

            SPContentType contentTypeWithField = FindContentTypeWithField(fieldToSet.ParentList.ContentTypes, fieldToSet);
            SPContentTypeId contentTypeId = contentTypeWithField.Id;

            tempItem[SPBuiltInFieldId.ContentTypeId] = contentTypeId;
            tempItem.Update();

            // re-fetch temp item, now with proper content types
            tempItem = fieldToSet.ParentList.GetItemById(tempItem.ID);

            var itemField = tempItem.Fields[fieldToSet.Id] as TaxonomyField;
    
            TaxonomyFieldValueCollection fieldValues = new TaxonomyFieldValueCollection(fieldToSet);

            foreach (Term term in this.Terms)
            {
                TaxonomyFieldValue fieldValue = new TaxonomyFieldValue(fieldToSet);

                fieldValue.TermGuid = term.Id.ToString();
                fieldValue.Label = term.Name;
                fieldValues.Add(fieldValue);
            }

            // Force population of field values to hit the TaxonomyHiddenList and generate some WSSid's
            itemField.SetFieldValue(tempItem, fieldValues);

            // Those taxonomy field values in the collection don't have a proper ValidatedString, but their WSSid's have been populated
            TaxonomyFieldValueCollection finalValue = tempItem[itemField.InternalName] as TaxonomyFieldValueCollection;
            
            // Clean up the temporary item
            tempFile.Delete();

            return finalValue;
        }
    }
}
