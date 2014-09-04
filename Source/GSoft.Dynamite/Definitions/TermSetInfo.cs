using System.Collections.Generic;

namespace GSoft.Dynamite.Definitions
{
    /// <summary>
    /// Encapsulates Taxonomy Term Set properties
    /// </summary>
    public class TermSetInfo
    {
        /// <summary>
        /// Default constructor for TermSetInfo
        /// </summary>
        public TermSetInfo()
        {}

        /// <summary>
        /// Labels by languages (LCID) for the Term Set
        /// </summary>
        public IDictionary<int, string> Labels { get; set; }
    }
}
