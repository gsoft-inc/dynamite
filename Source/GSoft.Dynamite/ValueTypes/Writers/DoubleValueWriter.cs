using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes double-based values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class DoubleValueWriter : BaseValueWriter<double?>
    {
        /// <summary>
        /// Writes a string field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var typedFieldValue = (double?)fieldValueInfo.Value;

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
        /// Writes a double value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (double?)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue.HasValue)
            {
                field.DefaultValue = defaultValue.Value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                field.DefaultValue = null;
            }
        }

        /// <summary>
        /// Writes a standard field value as an SPFolder's default value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update the column metadata defaults</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (double?)fieldValueInfo.Value;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            if (defaultValue.HasValue)
            {
                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, defaultValue.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();  
        }
    }
}