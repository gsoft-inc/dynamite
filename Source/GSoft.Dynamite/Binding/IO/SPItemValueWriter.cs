using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Has the responsibility to write values to a SharePoint list item.
    /// </summary>
    public class SPItemValueWriter : ISPItemValueWriter
    {
        private readonly SPItemBaseValueWriter itemBaseValueWriter;
        private readonly SPItemTaxonomyValueWriter itemTaxonomyValueWriter;
        private readonly SPItemTaxonomyMultiValueWriter itemTaxonomyMultiValueWriter;
        private readonly SPItemLookupValueWriter itemLookupValueWriter;
        private readonly SPItemPrincipalValueWriter itemPrincipalValueWriter;
        private readonly SPItemUserValueWriter itemUserValueWriter;
        private readonly SPItemUrlValueWriter itemUrlValueWriter;
        private readonly SPItemImageValueWriter itemImageValueWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SPItemValueWriter"/> class.
        /// </summary>
        /// <param name="itemBaseValueWriter">The base value writer.</param>
        /// <param name="itemTaxonomyValueWriter">The taxonomy value writer.</param>
        /// <param name="itemTaxonomyMultiValueWriter">The taxonomy multi value writer.</param>
        /// <param name="itemLookupValueWriter">The lookup value writer.</param>
        /// <param name="itemPrincipalValueWriter">The principal value writer.</param>
        /// <param name="itemUserValueWriter">The user value writer.</param>
        /// <param name="itemUrlValueWriter">The URL value writer.</param>
        /// <param name="itemImageValueWriter">The image value writer.</param>
        public SPItemValueWriter(
            SPItemBaseValueWriter itemBaseValueWriter,
            SPItemTaxonomyValueWriter itemTaxonomyValueWriter,
            SPItemTaxonomyMultiValueWriter itemTaxonomyMultiValueWriter,
            SPItemLookupValueWriter itemLookupValueWriter,
            SPItemPrincipalValueWriter itemPrincipalValueWriter,
            SPItemUserValueWriter itemUserValueWriter,
            SPItemUrlValueWriter itemUrlValueWriter,
            SPItemImageValueWriter itemImageValueWriter)
        {
            this.itemBaseValueWriter = itemBaseValueWriter;
            this.itemTaxonomyValueWriter = itemTaxonomyValueWriter;
            this.itemTaxonomyMultiValueWriter = itemTaxonomyMultiValueWriter;
            this.itemLookupValueWriter = itemLookupValueWriter;
            this.itemPrincipalValueWriter = itemPrincipalValueWriter;
            this.itemUserValueWriter = itemUserValueWriter;
            this.itemUrlValueWriter = itemUrlValueWriter;
            this.itemImageValueWriter = itemImageValueWriter;
        }

        /// <summary>
        /// Updates the given SPListItem with the values passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item.</param>
        /// <param name="fieldValueInfos">The value information to be updated in the SPListItem.</param>
        /// <returns>The updated SPListItem.</returns>
        public SPListItem WriteValuesToSPListItem(SPListItem item, IList<FieldValueInfo> fieldValueInfos)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (fieldValueInfos == null)
            {
                throw new ArgumentNullException("fieldValueInfos");
            }

            foreach (var fieldValue in fieldValueInfos)
            {
                item = this.WriteValueToSPListItem(item, fieldValue);
            }

            return item;
        }

        /// <summary>
        /// Updates the given SPListItem with the value passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item.</param>
        /// <param name="fieldValueInfo">The value information to be updated in the SPListItem.</param>
        /// <returns>The updated SPListItem.</returns>
        public SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (fieldValueInfo == null || fieldValueInfo.FieldInfo == null)
            {
                throw new ArgumentNullException("fieldValueInfo");
            }

            var associatedValueType = fieldValueInfo.FieldInfo.AssociatedValueType;
            if (associatedValueType == typeof(LookupValue))
            {
                return this.itemLookupValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            if (associatedValueType == typeof(LookupValueCollection))
            {
                throw new NotSupportedException("The value type 'LookupValueCollection' is not yet supported when writing to a SPListItem.");                
            }
            else if (associatedValueType == typeof(PrincipalValue))
            {
                return this.itemPrincipalValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            else if (associatedValueType == typeof(UserValue))
            {
                return this.itemUserValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            else if (associatedValueType == typeof(UserValueCollection))
            {
                throw new NotSupportedException("The value type 'UserValueCollection' is not yet supported when writing to a SPListItem.");
            }
            else if (associatedValueType == typeof(UrlValue))
            {
                return this.itemUrlValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            else if (associatedValueType == typeof(TaxonomyFullValue))
            {
                return this.itemTaxonomyValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            else if (associatedValueType == typeof(TaxonomyFullValueCollection))
            {
                return this.itemTaxonomyMultiValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }
            else if (associatedValueType == typeof(ImageValue))
            {
                return this.itemImageValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
            }

            return this.itemBaseValueWriter.WriteValueToSPListItem(item, fieldValueInfo);
        }
    }
}