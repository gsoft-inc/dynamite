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
        /// Default constructor for TermGroupInfo
        /// </summary>
        public TermGroupInfo()
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

        /// <summary>
        /// Term Sets for this group
        /// </summary>
        public IDictionary<string, TermSetInfo> TermSets { get; set; }
    }
}
