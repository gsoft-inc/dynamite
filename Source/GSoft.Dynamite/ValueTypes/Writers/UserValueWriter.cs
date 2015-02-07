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
    /// Writes user values to SharePoint list items, field definition's DefaultValue
    /// and folder MetadataDefaults.
    /// </summary>
    public class UserValueWriter : BaseValueWriter<UserValue>
    {
        /// <summary>
        /// Writes a user field value to a SPListItem
        /// </summary>
        /// <param name="item">The SharePoint List Item</param>
        /// <param name="fieldValueInfo">The field and value information</param>
        public override void WriteValueToListItem(SPListItem item, FieldValueInfo fieldValueInfo)
        {
            var userValue = fieldValueInfo.Value as UserValue;
            var newUserValue = userValue != null
                ? string.Format(CultureInfo.InvariantCulture, "{0};#{1}", userValue.Id, (userValue.DisplayName ?? string.Empty).Replace(";", ";;"))
                : null;

            item[fieldValueInfo.FieldInfo.InternalName] = newUserValue;
        }

        /// <summary>
        /// Writes a User value as an SPField's default value
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