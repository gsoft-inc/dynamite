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
    /// Writes Principal values to SharePoint list items.
    /// </summary>
    public class SPItemPrincipalValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a Principal field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var principal = fieldValueInfo.Value as PrincipalValue;
            var newValue = principal != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", principal.Id, (principal.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newValue;

            return item;
        }
    }
}