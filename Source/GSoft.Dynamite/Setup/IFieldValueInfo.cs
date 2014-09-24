namespace GSoft.Dynamite.Setup
{
    using Microsoft.Office.DocumentManagement;
    using Microsoft.SharePoint;
    using System;

    /// <summary>
    /// Metadata about a field value on a list item
    /// </summary>
    [Obsolete]
    public interface IFieldValueInfo
    {
        /// <summary>
        /// The field name
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// The value for english items
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Applies the value on the item
        /// </summary>
        /// <param name="item">The item to apply the value on</param>
        void ApplyOnItem(SPListItem item);

        /// <summary>
        /// Applies the value on a metadata default object
        /// </summary>
        /// <param name="metadata">The folder metadata defaults</param>
        /// <param name="folder">The folder in question</param>
        void ApplyFieldOnMetadata(MetadataDefaults metadata, SPFolder folder);
    }
}
