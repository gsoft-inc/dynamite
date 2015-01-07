using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Writes url values to SharePoint list items.
    /// </summary>
    public class SPItemUrlValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a url field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var urlValue = fieldValueInfo.Value as UrlValue;
            var newUrlValue = urlValue != null ? new SPFieldUrlValue { Url = urlValue.Url, Description = urlValue.Description } : null;
            item[fieldValueInfo.FieldInfo.InternalName] = newUrlValue;

            return item;
        }
    }
}