using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Lookup values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class LookupValueWriter : BaseValueWriter<LookupValue>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new <see cref="LookupValueWriter"/>
        /// </summary>
        /// <param name="log">Logging utility</param>
        public LookupValueWriter(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// Writes a lookup field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var lookup = fieldValueInfo.Value as LookupValue;

            item[fieldValueInfo.FieldInfo.InternalName] = lookup != null ? new SPFieldLookupValue(lookup.Id, lookup.Value) : null;
        }

        /// <summary>
        /// Writes a Lookup value as an SPField's default value
        /// WARNING: This should only be used in scenarios where you have complete and exclusive programmatic
        /// access to item creation - because SharePoint has patchy support for this and your NewForm.aspx pages WILL break. 
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (LookupValue)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue != null)
            {
                field.DefaultValue = new SPFieldLookupValue(defaultValue.Id, defaultValue.Value).ToString();

                this.log.Warn(
                    "Default value ({0}) set on field {1} with type Lookup. SharePoint does not support default values on Lookup fields through its UI."
                    + " Only list items created programmatically will get the default value properly set. Setting a Lookup-field default value"
                    + " will not be respected by your lists' NewForm.aspx item creation form.",
                    field.DefaultValue,
                    field.InternalName);
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
                    "WriteValueToFolderDefault - Initializing a folder column default value with LookupValue is not supported (fieldName={0}).",
                    fieldValueInfo.FieldInfo.InternalName));
        }
    }
}