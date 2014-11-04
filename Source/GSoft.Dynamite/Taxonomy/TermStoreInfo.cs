using System;
using System.Collections.Generic;

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
        /// Id of the group
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }
    }
}
