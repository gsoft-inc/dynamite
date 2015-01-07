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
    /// Writes Lookup values to SharePoint list items.
    /// </summary>
    public class SPItemLookupValueWriter : SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a lookup field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>
        /// The updated SPListItem.
        /// </returns>
        public override SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var lookup = fieldValueInfo.Value as LookupValue;

            item[fieldValueInfo.FieldInfo.InternalName] = lookup != null ? new SPFieldLookupValue(lookup.Id, lookup.Value) : null;

            return item;
        }
    }
}