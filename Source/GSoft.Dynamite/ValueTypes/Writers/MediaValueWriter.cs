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
    /// Writes rich media (video, audio) values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class MediaValueWriter : BaseValueWriter<MediaValue>
    {
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
            var withDefaultVal = (FieldInfo<MediaValue>)fieldValueInfo.FieldInfo;
            var field = parentFieldCollection[fieldValueInfo.FieldInfo.Id];

            if (withDefaultVal.DefaultValue != null)
            {
                var imageValue = withDefaultVal.DefaultValue;
                var sharePointFieldMediaValue = CreateSharePointMediaFieldValue(imageValue);

                field.DefaultValue = sharePointFieldMediaValue.ToString();
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