using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Encapsulates Taxonomy Term Store properties
    /// </summary>
    public class TermStoreInfo
    {
        /// <summary>
        /// Default constructor for TermStoreInfo for serialization purposes
        /// </summary>
        public TermStoreInfo()
        {           
        }
        
        /// <summary>
        /// Constructor for TermStoreInfo
        /// </summary>
        /// <param name="id">The term store's ID</param>
        /// <param name="name">The term store's name</param>
        public TermStoreInfo(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        /// <summary>
        /// Convenience constructor to create TermStoreInfo objects
        /// from SharePoint TermStore instances
        /// </summary>
        /// <param name="sharePointTermStore">The SharePoint taxonomy store</param>
        public TermStoreInfo(TermStore sharePointTermStore)
        {
            this.Id = sharePointTermStore.Id;
            this.Name = sharePointTermStore.Name;
        }

        /// <summary>
        /// Id of the group
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }
    }
}
