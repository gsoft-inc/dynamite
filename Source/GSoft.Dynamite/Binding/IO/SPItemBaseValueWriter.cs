using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Writes standard values to SharePoint list items.
    /// </summary>
    public class SPItemBaseValueWriter
    {
        /// <summary>
        /// Writes a standard field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        /// <returns>The updated SPListItem.</returns>
        public virtual SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            item[fieldValueInfo.FieldInfo.InternalName] = fieldValueInfo.Value;

            return item;
        }
    }
}