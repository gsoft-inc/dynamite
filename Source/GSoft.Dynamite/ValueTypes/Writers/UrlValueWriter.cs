using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
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
            var withDefaultVal = (FieldInfo<UrlValue>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue != null)
            {
                var urlValue = withDefaultVal.DefaultValue;

                var newUrlValue = new SPFieldUrlValue { Url = urlValue.Url, Description = urlValue.Description };

                // Avoid setting the Description as well, otherwise all
                // new items created with that field will have both the URL
                // and Description in their URL and Description fields (weird lack
                // of OOTB support for Url default values).
                field.DefaultValue = newUrlValue.Url;

                if (!string.IsNullOrEmpty(urlValue.Description))
                {
                    this.log.Warn(
                        "Skipped initialization of Description property (val={0}) on Url field value (urlval={1}). "
                        + "A SPFieldUrlValue cannot support more than a simple URL string as default value.", 
                        urlValue.Description, 
                        urlValue.Url);
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
        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }
    }
}