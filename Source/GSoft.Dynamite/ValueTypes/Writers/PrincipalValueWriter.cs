using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Principal values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class PrincipalValueWriter : BaseValueWriter<PrincipalValue>
    {
        /// <summary>
        /// Writes a Principal field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var principal = fieldValueInfo.Value as PrincipalValue;
            var newValue = principal != null ? FormatPrincipalString(principal) : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newValue;
        }

        /// <summary>
        /// Writes a Principal value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var withDefaultVal = (FieldInfo<PrincipalValue>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue != null)
            {
                field.DefaultValue = FormatPrincipalString(withDefaultVal.DefaultValue);
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a field value as an SPFolder's default column value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update a field's default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }

        private static string FormatPrincipalString(PrincipalValue principalValue)
        {
            return string.Format(
                CultureInfo.InvariantCulture, 
                "{0};#{1}", 
                principalValue.Id, 
                (principalValue.DisplayName ?? string.Empty).Replace(";", ";;"));
        }
    }
}