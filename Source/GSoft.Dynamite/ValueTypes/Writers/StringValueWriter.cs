using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Fields.Types;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Logging;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes string-based values to SharePoint list items.
    /// </summary>
    public class StringValueWriter : BaseValueWriter<string>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new string-based field value writer
        /// </summary>
        /// <param name="log">Logging utility</param>
        public StringValueWriter(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// Writes a string field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            item[fieldValueInfo.FieldInfo.InternalName] = fieldValueInfo.Value;
        }

        /// <summary>
        /// Writes a string value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (string)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (fieldValueInfo.FieldInfo is NoteFieldInfo || fieldValueInfo.FieldInfo is HtmlFieldInfo)
            {
                this.log.Warn(
                    "WriteValueToFieldDefault - Initializing {0} field (fieldName={0}) with default value \"{1}\"."
                    + " Be aware that field default values on {0}-type field are not well supported by SharePoint and that this default"
                    + " value will not be editable through your site column's settings page.",
                    fieldValueInfo.FieldInfo.Type,
                    fieldValueInfo.FieldInfo.InternalName,
                    defaultValue);
            }

            field.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update the column metadata defaults</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (string)fieldValueInfo.Value;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            var parentList = folder.ParentWeb.Lists[folder.ParentListId];

            // Pages library is a special case: attempting to set folder default value on any text-based field (Text, Note or HTML)
            // will always fail because of patchy OOTB support.
            if ((int)parentList.BaseTemplate == BuiltInListTemplates.Pages.ListTempateTypeId
                && !string.IsNullOrEmpty(defaultValue))
            {
                string exceptionMessage = "WriteValueToFolderDefault - Impossible to set folder default value (val={0}) on field {1} of type {2}"
                    + " within the Pages library. That column default would be ignored.";

                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        exceptionMessage,
                        defaultValue,
                        fieldValueInfo.FieldInfo.InternalName,
                        fieldValueInfo.FieldInfo.Type));
            }

            if (defaultValue != null)
            {
                if (fieldValueInfo.FieldInfo is NoteFieldInfo || fieldValueInfo.FieldInfo is HtmlFieldInfo)
                {
                    this.log.Warn(
                        "WriteValueToFolderDefault - Initializing {0} field (fieldName={1}) with default value \"{2}\"."
                        + " Be aware that folder default values on {0}-type field are not well supported by SharePoint and that this default" 
                        + " value will not be editable through your document library's \"List Settings > Column default value settings\" options page.",
                        fieldValueInfo.FieldInfo.Type,
                        fieldValueInfo.FieldInfo.InternalName,
                        defaultValue);
                }

                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, defaultValue);
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();
        }
    }
}