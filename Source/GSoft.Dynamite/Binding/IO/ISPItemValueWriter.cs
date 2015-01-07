using System;
using System.Collections.Generic;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Binding.IO
{
    /// <summary>
    /// Handlers writing values to a SharePoint list item.
    /// </summary>
    public interface ISPItemValueWriter
    {
        /// <summary>
        /// Updates the given SPListItem with the values passed.
        /// </summary>
        /// <param name="item">The SharePoint list item.</param>
        /// <param name="fieldValues">The values to be updated in the SPListItem.</param>
        /// <returns>The updated SPListItem.</returns>
        SPListItem WriteValuesToSPListItem(SPListItem item, IList<FieldValueInfo> fieldValues);

        /// <summary>
        /// Updates the given SPListItem with the value passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item.</param>
        /// <param name="fieldValueInfos">The value information to be updated in the SPListItem.</param>
        /// <returns>The updated SPListItem.</returns>
        SPListItem WriteValueToSPListItem(SPListItem item, FieldValueInfo fieldValueInfo);
    }
}