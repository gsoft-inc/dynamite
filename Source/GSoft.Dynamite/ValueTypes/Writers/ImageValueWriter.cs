using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
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
                sharePointFieldImageValue = this.CreateSharePointImageFieldValue(imageValue);
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
            var withDefaultVal = (FieldInfo<ImageValue>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue != null)
            {
                var imageValue = withDefaultVal.DefaultValue;
                var sharePointFieldImageValue = this.CreateSharePointImageFieldValue(imageValue);

                field.DefaultValue = sharePointFieldImageValue.ToString();
            }
        }

        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }

        private ImageFieldValue CreateSharePointImageFieldValue(ImageValue imageValue)
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