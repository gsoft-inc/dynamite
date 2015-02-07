using System;
using System.Collections.Generic;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Handlers writing values to a SharePoint list item.
    /// </summary>
    public interface IFieldValueWriter
    {
        /// <summary>
        /// Updates the given SPListItem with the values passed.
        /// </summary>
        /// <param name="item">The SharePoint list item to update.</param>
        /// <param name="fieldValueInfos">The values to be updated in the SPListItem.</param>
        void WriteValuesToListItem(SPListItem item, IList<FieldValueInfo> fieldValueInfos);

        /// <summary>
        /// Updates the given SPListItem with the value passed.
        /// This method does not call Update or SystemUpdate.
        /// </summary>
        /// <param name="item">The SharePoint list item to update.</param>
        /// <param name="fieldValueInfo">The value information to be updated in the SPListItem.</param>
        void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo);

        void WriteValuesToFieldDefaults(SPFieldCollection field, IList<FieldValueInfo> fieldValueInfos);

        void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo);

        void WriteValuesToFolderDefaults(SPFolder folder, IList<FieldValueInfo> fieldValueInfos);

        void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo);
    }
}