using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;

namespace GSoft.Dynamite.Setup
{
    /// <summary>
    /// Helps in filling fields
    /// </summary>
    public class FieldValueInfo : IFieldValueInfo
    {
        /// <summary>
        /// The field name
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// The value for english items
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Applies the on item.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void ApplyOnItem(SPListItem item)
        {
            item[this.FieldName] = this.Value;
        }

        /// <summary>
        /// Applies the field on metadata.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="folder">The folder.</param>
        public virtual void ApplyFieldOnMetadata(MetadataDefaults metadata, SPFolder folder)
        {
            var defaultValue = string.Empty;

            if (this.Value != null)
            {
                defaultValue = this.Value.ToString();
            }

            metadata.SetFieldDefault(
                folder,
                this.FieldName,
                defaultValue);

            metadata.Update();
        }
    }
}
