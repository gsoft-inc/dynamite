using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Office.DocumentManagement;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Setup
{    
    /// <summary>
    /// Metadata about a taxonomy-multi field value
    /// </summary>
    public interface ITaxonomyMultiInfo : IFieldValueInfo
    {
        /// <summary>
        /// The multiple terms for the field value
        /// </summary>
        Collection<Term> Terms { get; set; }

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
