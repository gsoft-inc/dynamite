using System;
using System.Collections.Generic;

namespace GSoft.Dynamite.Definitions
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
        public TermGroupInfo(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.TermStore = null;      // should assume default term store
        }

        /// <summary>
        /// Constructor for TermGroupInfo belonging to specifc term store
        /// </summary>
        public TermGroupInfo(Guid id, string name, TermStoreInfo termStore)
        {
            this.Id = id;
            this.Name = name;
            this.TermStore = termStore;      // specific term store
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
        /// Parent term store definition
        /// </summary>
        public TermStoreInfo TermStore { get; set; }
    }
}
