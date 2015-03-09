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
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes rich media (video, audio) values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class MediaValueWriter : BaseValueWriter<MediaValue>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new <see cref="MediaValueWriter"/>
        /// </summary>
        /// <param name="log">Logging utility</param>
        public MediaValueWriter(ILogger log)
        {
            this.log = log;
        }

        /// <summary>
        /// Writes an image field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var mediaValue = fieldValueInfo.Value as MediaValue;

            MediaFieldValue sharePointFieldMediaValue = null;

            if (mediaValue != null)
            {
                sharePointFieldMediaValue = CreateSharePointMediaFieldValue(mediaValue);
            }

            item[fieldValueInfo.FieldInfo.InternalName] = sharePointFieldMediaValue;
        }
        
        /// <summary>
        /// Writes a publishing Image value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (MediaValue)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue != null)
            {
                var sharePointFieldMediaValue = CreateSharePointMediaFieldValue(defaultValue);

                field.DefaultValue = sharePointFieldMediaValue.ToString();
            }
            else if (field.DefaultValue != null)
            {
                // Setting SPField.DefaultValue to NULL will always end up setting it as string.Empty.
                // The Media field type behaves weirdly when string.Empty is its DefaultValue (the NewForm.aspx breaks
                // because of impossible cast from string to MediaFieldValue type).
                // Thus, if the DefaultValue was already NULL, we gotta be carefull not to replace that NULL with an
                // empty string needlessly.
                this.log.Warn(
                    "WriteValueToFieldDefault - Initializing {0} field (fieldName={0}) with default value \"{1}\"."
                    + " Be aware that folder default values on {0}-type field are not well supported by SharePoint and that this default"
                    + " value will not be editable through your document library's \"List Settings > Column default value settings\" options page.",
                    fieldValueInfo.FieldInfo.FieldType,
                    fieldValueInfo.FieldInfo.InternalName,
                    defaultValue);

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
            var defaultValue = (MediaValue)fieldValueInfo.Value;
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            if (defaultValue != null)
            {
                var sharePointFieldMediaValue = CreateSharePointMediaFieldValue(defaultValue);

                this.log.Warn(
                    "WriteValueToFolderDefault - Initializing {0} field (fieldName={1}) with default value \"{2}\"."
                    + " Be aware that folder default values on {0}-type field are not well supported by SharePoint and that this default"
                    + " value will not be editable through your document library's \"List Settings > Column default value settings\" options page.",
                    fieldValueInfo.FieldInfo.FieldType,
                    fieldValueInfo.FieldInfo.InternalName,
                    sharePointFieldMediaValue.ToString());

                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, sharePointFieldMediaValue.ToString());
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();
        }

        private static MediaFieldValue CreateSharePointMediaFieldValue(MediaValue mediaVal)
        {
            var fieldValue = new MediaFieldValue()
            {
                Title = mediaVal.Title,
                MediaSource = mediaVal.Url,
                PreviewImageSource = mediaVal.PreviewImageUrl,
                DisplayMode = mediaVal.DisplayMode,
                TemplateSource = mediaVal.XamlTemplateUrl,
                InlineHeight = mediaVal.InlineHeight,
                InlineWidth = mediaVal.InlineWidth,
                AutoPlay = mediaVal.IsAutoPlay,
                Loop = mediaVal.IsLoop
            };

            return fieldValue;
        }
    }
}