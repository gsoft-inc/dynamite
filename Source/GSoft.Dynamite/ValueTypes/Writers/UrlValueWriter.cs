using System;
using System.Collections.Generic;
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
    /// Writes url values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class UrlValueWriter : BaseValueWriter<UrlValue>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new <see cref="UrlValueWriter"/>
        /// </summary>
        /// <param name="log">Logging utility</param>
        public UrlValueWriter(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// Writes a url field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var urlValue = fieldValueInfo.Value as UrlValue;
            var newUrlValue = urlValue != null ? new SPFieldUrlValue { Url = urlValue.Url, Description = urlValue.Description } : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newUrlValue;
        }

        /// <summary>
        /// Writes a URL value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (UrlValue)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue != null)
            {
                var newUrlValue = new SPFieldUrlValue { Url = defaultValue.Url, Description = defaultValue.Description };

                // Avoid setting the Description as well, otherwise all
                // new items created with that field will have both the URL
                // and Description in their URL and Description fields (weird lack
                // of OOTB support for Url default values).
                field.DefaultValue = newUrlValue.Url;

                if (!string.IsNullOrEmpty(defaultValue.Description))
                {
                    this.log.Warn(
                        "WriteValueToFieldDefault - Skipped initialization of Description property (val={0}) on Url field value (urlval={1})."
                        + " A SPFieldUrlValue cannot support more than a simple URL string as default value for your field {2}."
                        + " Be aware that field default values on \"Hyperlink or Picture\"-type field are not well supported by SharePoint"
                        + " and that this default value will not be editable through your site column's settings page.",
                        defaultValue.Description,
                        defaultValue.Url,
                        fieldValueInfo.FieldInfo.InternalName);
                }
                else
                {
                    this.log.Warn(
                        "WriteValueToFieldDefault - Be aware that field default values on \"Hyperlink or Picture\"-type field are not well supported by SharePoint"
                        + " and that this default value will not be editable through your site column's settings page (fieldName={0}) defaultVal={1}).",
                    fieldValueInfo.FieldInfo.InternalName,
                    newUrlValue.Url);
                }
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
            var defaultValue = (UrlValue)fieldValueInfo.Value;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            if (defaultValue != null)
            {
                var sharePointFieldUrlValue = new SPFieldUrlValue { Url = defaultValue.Url, Description = defaultValue.Description };

                this.log.Warn(
                    "WriteValueToFolderDefault - Initializing {0} field (fieldName={1}) with default value \"{2}\"."
                    + " Be aware that folder default values on {0}-type field are not well supported by SharePoint and that this default"
                    + " value will not be editable through your document library's \"List Settings > Column default value settings\" options page.",
                    fieldValueInfo.FieldInfo.Type,
                    fieldValueInfo.FieldInfo.InternalName,
                    sharePointFieldUrlValue.ToString());

                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, sharePointFieldUrlValue.ToString());
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update(); 
        }
    }
}