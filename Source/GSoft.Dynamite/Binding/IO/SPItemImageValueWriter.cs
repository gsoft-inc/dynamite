using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Writes image values to SharePoint list items.
    /// </summary>
    public class SPItemImageValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes an image field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var imageValue = fieldValueInfo.Value as ImageValue;

            ImageFieldValue newImageValue = null;

            if (imageValue != null)
            {
                newImageValue = new ImageFieldValue()
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
            }

            item[fieldValueInfo.FieldInfo.InternalName] = newImageValue;

            return item;
        }
    }
}