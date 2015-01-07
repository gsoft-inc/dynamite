using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Writes user values to SharePoint list items.
    /// </summary>
    public class SPItemUserValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a user field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var userValue = fieldValueInfo.Value as UserValue;
            var newUserValue = userValue != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", userValue.Id, (userValue.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newUserValue;

            return item;
        }
    }
}