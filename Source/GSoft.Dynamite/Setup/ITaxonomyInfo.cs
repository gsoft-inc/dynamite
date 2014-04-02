namespace GSoft.Dynamite.Setup
{
    using Microsoft.Office.DocumentManagement;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Taxonomy;

    /// <summary>
    /// Metadata about a taxonomy field value
    /// </summary>
    public interface ITaxonomyInfo : IFieldValueInfo
    {
        /// <summary>
        /// The related term
        /// </summary>
        Term Term { get; set; }

        /// <summary>
        /// Applies the taxonomy value on the item
        /// </summary>
        /// <param name="item">The item to apply on</param>
        /// <param name="list">The list</param>
        void ApplyOnItem(SPListItem item, SPList list);

        /// <summary>
        /// Applies the taxonomy value on folder metadata defaults object
        /// </summary>
        /// <param name="metadata">The folder metadata defaults</param>
        /// <param name="folder">The folder</param>
        /// <param name="list">The list</param>
        void ApplyFieldOnMetadata(MetadataDefaults metadata, SPFolder folder, SPList list);
    }
}
