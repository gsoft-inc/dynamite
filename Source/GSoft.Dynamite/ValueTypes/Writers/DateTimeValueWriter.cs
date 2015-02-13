using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Logging;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes DateTime-based values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class DateTimeValueWriter : BaseValueWriter<DateTime?>
    {
        /// <summary>
        /// Writes a string field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var typedFieldValue = (DateTime?)fieldValueInfo.Value;

            if (typedFieldValue.HasValue)
            {
                item[fieldValueInfo.FieldInfo.InternalName] = typedFieldValue.Value.ToUniversalTime();
            }
            else
            {
                item[fieldValueInfo.FieldInfo.InternalName] = null;
            }            
        }
        
        /// <summary>
        /// Writes a boolean value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (DateTime?)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue.HasValue)
            {
                field.DefaultValue = FormatLocalDateTimeString(defaultValue.Value);
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The field for which we wish to update the default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (DateTime?)fieldValueInfo.Value;
            var list = folder.ParentWeb.Lists[folder.ParentListId];
            var listField = list.Fields[fieldValueInfo.FieldInfo.Id];
            bool isPagesLibrary = (int)list.BaseTemplate == BuiltInListTemplates.Pages.ListTempateTypeId;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(list);

            // Pages library is a special case: attempting to set default value to TRUE will
            // always fail because of patchy OOTB support.
            if (isPagesLibrary
                && defaultValue.HasValue
                && (!string.IsNullOrEmpty(listField.DefaultValue) || !string.IsNullOrEmpty(listField.DefaultFormula)))
            {
                string exceptionMessage = "WriteValueToFolderDefault - Impossible to set folder default value as on DateTime-type field (fieldName={0})"
                    + " within the Pages library when the SPField already has a DefaultValue or DefaultFormula. That folder column default (val={1})"
                    + "would be ignored.";

                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        exceptionMessage,
                        fieldValueInfo.FieldInfo.InternalName,
                        defaultValue.Value.ToString(CultureInfo.InvariantCulture)));
            }

            if (defaultValue.HasValue)
            {
                // Weirdness warning: between regular Document Libraries and the Pages Library,
                // how we set DateTime column default per-folder needs to be different.
                // On Document Library folder, we need to convert the DateTime value to UTC before
                // we assign it as a default column value (i.e. we need to go from local time to UTC).
                // On a Pages library folder, we need to set the local datetime string as the default,
                // without UTC conversion.
                DateTime defaultValueWithUTCConversionIfNeeded = isPagesLibrary ? defaultValue.Value : defaultValue.Value.ToUniversalTime();
                string dateString = FormatLocalDateTimeString(defaultValueWithUTCConversionIfNeeded);
                listMetadataDefaults.SetFieldDefault(folder.ServerRelativeUrl, fieldValueInfo.FieldInfo.InternalName, dateString);
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder.ServerRelativeUrl, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();      
        }

        private static string FormatLocalDateTimeString(DateTime dateTime)
        {
            return SPUtility.CreateISO8601DateTimeFromSystemDateTime(dateTime);
        }
    }
}