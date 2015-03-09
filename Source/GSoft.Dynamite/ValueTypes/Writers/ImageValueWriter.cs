using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.Lists.Constants;
using GSoft.Dynamite.Logging;
using GSoft.Dynamite.ValueTypes;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes image values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class ImageValueWriter : BaseValueWriter<ImageValue>
    {
        private ILogger log;

        /// <summary>
        /// Creates a new <see cref="ImageValueWriter"/>
        /// </summary>
        /// <param name="log">Logging utility</param>
        public ImageValueWriter(ILogger log)
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
            var imageValue = fieldValueInfo.Value as ImageValue;

            ImageFieldValue sharePointFieldImageValue = null;

            if (imageValue != null)
            {
                sharePointFieldImageValue = CreateSharePointImageFieldValue(imageValue);
            }

            item[fieldValueInfo.FieldInfo.InternalName] = sharePointFieldImageValue;
        }
        
        /// <summary>
        /// Writes a publishing Image value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            var defaultValue = (ImageValue)fieldValueInfo.Value;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (defaultValue != null)
            {
                var sharePointFieldImageValue = CreateSharePointImageFieldValue(defaultValue);

                this.log.Warn(
                    "WriteValueToFieldDefault - Be aware that field default values on \"Image\"-type field are not well supported by SharePoint"
                    + " and that this default value will not be editable through your site column's settings page (fieldName={0}, defaultVal={1}).",
                    fieldValueInfo.FieldInfo.InternalName,
                    sharePointFieldImageValue.ToString());

                field.DefaultValue = sharePointFieldImageValue.ToString();
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
            var defaultValue = (ImageValue)fieldValueInfo.Value;
            var list = folder.ParentWeb.Lists[folder.ParentListId];
            MetadataDefaults listMetadataDefaults = new MetadataDefaults(folder.ParentWeb.Lists[folder.ParentListId]);

            // Pages library is a special case: attempting to set default value will
            // always fail because of patchy OOTB support.
            if ((int)list.BaseTemplate == BuiltInListTemplates.Pages.ListTempateTypeId)
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "WriteValueToFolderDefault - Initializing a folder column default value with ImageValue within the Pages library s not supported (fieldName={0}).",
                        fieldValueInfo.FieldInfo.InternalName));
            }
          
            if (defaultValue != null)
            {
                var sharePointFieldImageValue = CreateSharePointImageFieldValue(defaultValue);

                this.log.Warn(
                    "WriteValueToFolderDefault - Initializing {0} field (fieldName={1}) with default value \"{2}\"."
                    + " Be aware that folder default values on {0}-type field are not well supported by SharePoint and that this default"
                    + " value will not be editable through your document library's \"List Settings > Column default value settings\" options page.",
                    fieldValueInfo.FieldInfo.FieldType,
                    fieldValueInfo.FieldInfo.InternalName,
                    sharePointFieldImageValue.ToString());
                
                listMetadataDefaults.SetFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName, sharePointFieldImageValue.ToString());
            }
            else
            {
                listMetadataDefaults.RemoveFieldDefault(folder, fieldValueInfo.FieldInfo.InternalName);
            }

            listMetadataDefaults.Update();    
        }

        private static ImageFieldValue CreateSharePointImageFieldValue(ImageValue imageValue)
        {
            var fieldImageValue = new ImageFieldValue()
            {
                Alignment = imageValue.Alignment,
                AlternateText = imageValue.AlternateText,
                BorderWidth = imageValue.BorderWidth,
                Height = imageValue.Height,
                HorizontalSpacing = imageValue.HorizontalSpacing,
                Hyperlink = imageValue.Hyperlink,
                ImageUrl = imageValue.ImageUrl,
                OpenHyperlinkInNewWindow = imageValue.OpenHyperlinkInNewWindow,
                VerticalSpacing = imageValue.VerticalSpacing,
                Width = imageValue.Width,
            };

            return fieldImageValue;
        }
    }
}