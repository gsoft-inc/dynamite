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
        /// This method does not call Update or SystemUpdate.
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

        /// <summary>
        /// Updates the specified SPField definitions with new DefaultValues
        /// </summary>
        /// <param name="parentFieldCollection">The SharePoint field collection containing the fields to update.</param>
        /// <param name="defaultFieldValueInfos">The default values to be applied as the SPFields' new defaults.</param>
        void WriteValuesToFieldDefaults(SPFieldCollection parentFieldCollection, IList<FieldValueInfo> defaultFieldValueInfos);

        /// <summary>
        /// Updates the specified SPField definition with new DefaultValue
        /// </summary>
        /// <param name="parentFieldCollection">The SharePoint field collection containing the field to update.</param>
        /// <param name="defaultFieldValueInfo">The default value to be applied as the SPField' new default.</param>
        void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo defaultFieldValueInfo);

        /// <summary>
        /// Updates the specified SPFolder with new default field values
        /// </summary>
        /// <param name="folder">The SharePoint folder for which we want to update the metadata defaults.</param>
        /// <param name="defaultFieldValueInfos">The default values to be applied to items created within that folder.</param>
        void WriteValuesToFolderDefaults(SPFolder folder, IList<FieldValueInfo> defaultFieldValueInfos);

        /// <summary>
        /// Updates the specified SPFolder with new default field value
        /// </summary>
        /// <param name="folder">The SharePoint folder for which we want to update the metadata defaults.</param>
        /// <param name="defaultFieldValueInfo">The default value to be applied to items created within that folder.</param>
        void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo defaultFieldValueInfo);
    }
}