using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using Microsoft.SharePoint;

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
            var withDefaultVal = (FieldInfo<bool?>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue.HasValue)
            {
                field.DefaultValue = withDefaultVal.DefaultValue.Value.ToString();

                this.log.Warn(
                    "Default value ({0}) set on field {1} with type Boolean. SharePoint does not support default values on Boolean fields. "
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
        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }
    }
}