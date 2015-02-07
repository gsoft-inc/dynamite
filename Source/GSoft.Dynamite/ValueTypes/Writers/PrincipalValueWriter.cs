using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSoft.Dynamite.Fields;
using GSoft.Dynamite.ValueTypes;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.ValueTypes.Writers
{
    /// <summary>
    /// Writes Principal values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class PrincipalValueWriter : BaseValueWriter<PrincipalValue>
    {
        /// <summary>
        /// Writes a Principal field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var principal = fieldValueInfo.Value as PrincipalValue;
            var newValue = principal != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", principal.Id, (principal.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newValue;
        }

        /// <summary>
        /// Writes a Principal value as an SPField's default value
        /// </summary>
        /// <param name="parentFieldCollection">The parent field collection within which we can find the specific field to update</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToFieldDefault(SPFieldCollection parentFieldCollection, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }

        public override void WriteValuesToFolderDefault(SPFolder folder, FieldValueInfo fieldValueInfo)
        {
            throw new NotImplementedException();
        }
    }
}