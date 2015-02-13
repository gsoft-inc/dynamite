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
using Microsoft.SharePoint.Publishing;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes boolean-based values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class BooleanValueWriter : BaseValueWriter<bool?>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new <see cref="BooleanValueWriter"/>
        /// </summary>
        /// <param name="log">Logging utility</param>
        public BooleanValueWriter(ILogger log)
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
            var typedFieldValue = (bool?)fieldValueInfo.Value;

            if (typedFieldValue.HasValue)
            {
                item[fieldValueInfo.FieldInfo.InternalName] = typedFieldValue.Value;
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
            var defaultValue = (bool?)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue.HasValue)
            {
                field.DefaultValue = defaultValue.Value.ToString();

                this.log.Warn(
                    "Default value ({0}) set on field {1} with type Boolean. SharePoint has patchy support for default values on Boolean fields. "
                    + "Only list items created programmatically will get the default value properly set. Setting a Boolean-field default value will not be "
                    + "respected in your lists' NewForm.aspx item creation form.",
                    field.DefaultValue,
                    field.InternalName);
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update the default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (bool?)fieldValueInfo.Value;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            var parentList = folder.ParentWeb.Lists[folder.ParentListId];
            var listField = parentList.Fields[fieldValueInfo.FieldInfo.Id];

            // Pages library is a special case: attempting to set default value to TRUE will
            // always fail because of patchy OOTB support.
            if ((int)parentList.BaseTemplate == BuiltInListTemplates.Pages.ListTempateTypeId
                && defaultValue.HasValue
                && defaultValue.Value)
            {
                string exceptionMessage = "WriteValueToFolderDefault - Impossible to set folder default value as TRUE"
                    + " within the Pages library. That column default would be ignored. (fieldName={0})";

                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        exceptionMessage,
                        fieldValueInfo.FieldInfo.InternalName));
            }

            if (!string.IsNullOrEmpty(listField.DefaultValue)
                && bool.Parse(listField.DefaultValue)
                && defaultValue.HasValue
                && !defaultValue.Value)
            {
                // The SPField already has a default value set to TRUE. Our folder column default FALSE will have no
                // effect because the field definition's default will always be applied. Thanks SharePoint!
                string exceptionMessage = "WriteValueToFolderDefault - The field {0} already has a DefaultValue=TRUE definition."
                    + " Your attempt to define a folder column default with value=FALSE would not work, since the TRUE"
                    + " value imposed by the SPField's DefaultValue will always \"win\" and be applied instead.";

                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        exceptionMessage,
                        fieldValueInfo.FieldInfo.InternalName));
            }

            if (defaultValue.HasValue)
            {
                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, defaultValue.Value.ToString());
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }   

            listMetadataDefaults.Update();   
        }
    }
}