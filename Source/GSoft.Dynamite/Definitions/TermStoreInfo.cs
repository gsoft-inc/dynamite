using System;
using System.Collections.Generic;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Encapsulates Taxonomy Term Store properties
    /// </summary>
    public class TermStoreInfo
    {
        /// <summary>
        /// Default constructor for TermGroupInfo
        /// </summary>
        public TermStoreInfo()
        {           
        }

        /// <summary>
        /// Id of the group
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }
    }
}
