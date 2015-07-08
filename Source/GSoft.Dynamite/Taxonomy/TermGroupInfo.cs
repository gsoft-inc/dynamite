using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Taxonomy;

namespace GSoft.Dynamite.Taxonomy
{
    /// <summary>
    /// Encapsulates Taxonomy Term Group properties
    /// </summary>
    public class TermGroupInfo
    {
        /// <summary>
        /// Default constructor for TermGroupInfo for serialization purposes
        /// </summary>
        public TermGroupInfo()
        {
        }

        /// <summary>
        /// Constructor for TermGroupInfo belonging to default Farm term store
        /// </summary>
        /// <param name="id">The term group id</param>
        /// <param name="name">The term group name</param>
        public TermGroupInfo(Guid id, string name) : this()
        {
            this.Name = name;
            this.Id = id;
            this.TermStore = null;
        }

        /// <summary>
        /// Constructor for TermGroupInfo belonging to specific term store
        /// </summary>
        /// <param name="id">The term group's ID</param>
        /// <param name="name">The term group's name</param>
        /// <param name="termStore">The parent term store</param>
        public TermGroupInfo(Guid id, string name, TermStoreInfo termStore) : this(id, name)
        {
            this.TermStore = termStore;
        }

        /// <summary>
        /// Convenience constructor to create TeermGroupInfo objects from
        /// SharePoint's taxonomy group instances
        /// </summary>
        /// <param name="sharePointTermGroup">The SharePoint taxonomy group</param>
        public TermGroupInfo(Group sharePointTermGroup)
        {
            this.Name = sharePointTermGroup.Name;
            this.Id = sharePointTermGroup.Id;
            this.TermStore = new TermStoreInfo(sharePointTermGroup.TermStore);
            this.IsSiteCollectionSpecificTermGroup = sharePointTermGroup.IsSiteCollectionGroup;
        }

        /// <summary>
        /// Id of the group
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the group (non-localizable)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Parent term store definition.
        /// If this value is null, assume that we are using the DefaultSiteCollectionTermStore.
        /// </summary>
        public TermStoreInfo TermStore { get; set; }

        /// <summary>
        /// True, if this is the Publishing automatic per-site-collection term group.
        /// False, if this is a farm-wide (typical) term group.
        /// </summary>
        public bool IsSiteCollectionSpecificTermGroup { get; set; }
    }
}
