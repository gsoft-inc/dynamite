using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Defines the generic contract for all ValueType writers
    /// </summary>
    public interface IBaseValueWriter
    {
        /// <summary>
        /// The ValueType with which the writer is compatible
        /// </summary>
        Type AssociatedValueType { get; }

        /// <summary>
        /// Writes a field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo);

        /// <summary>
        /// Writes a field value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo);

        /// <summary>
        /// Writes a field value as an SPFolder's default column value
        /// </summary>
        /// <param name="folder">The folder for which we wish to update a field's default value</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo);
    }
}