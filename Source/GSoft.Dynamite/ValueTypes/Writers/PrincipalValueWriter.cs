using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.Office.DocumentManagement;
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
            var newValue = principal != null ? FormatPrincipalString(item.Web, principal) : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newValue;
        }

        /// <summary>
        /// Writes a Principal value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (PrincipalValue)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue != null)
            {
                field.DefaultValue = FormatPrincipalString(parentFieldCollection.Web, defaultValue);
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
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotSupportedException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "WriteValueToFolderDefault - Initializing a folder column default value with PrincipalValue is not supported (fieldName={0}).",
                    fieldValueInfo.FieldInfo.InternalName));
        }

        private static string FormatPrincipalString(SPWeb web, PrincipalValue principalValue)
        {
            // Ensure User in SharePoint of the id is 0
            int userId = principalValue.Id;
            if(userId == 0)
            {
                userId = web.EnsureUser(principalValue.LoginName).ID;
            }

            return string.Format(
                CultureInfo.InvariantCulture, 
                "{0};#{1}",
                userId, 
                (principalValue.DisplayName ?? string.Empty).Replace(";", ";;"));
        }
    }
}